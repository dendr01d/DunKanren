﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Immutable;

namespace DunKanren
{
    public class State : IPrintable, IGrounded
    {
        public ImmutableDictionary<Variable, Instance> Subs;

        public int VariableCounter;
        public readonly int RecursionLevel;

        private int KeyWidth => Subs.Any() ? Subs.Keys.Max(x => x.Symbol.Length) : 0;

        private State()
        {
            this.Subs = ImmutableDictionary<Variable, Instance>.Empty;

            this.RecursionLevel = 0;
        }

        private State(State s, bool recursing) : this()
        {
            this.Subs = this.Subs.AddRange(s.Subs.ToDictionary(x => x.Key, x => x.Value));

            this.VariableCounter = s.VariableCounter;
            this.RecursionLevel = recursing ? s.RecursionLevel + 1 : s.RecursionLevel;
        }

        public static State InitialState() => new();

        public State Next() => new(this, true);

        public State Dupe() => new(this, false);

        /// <summary>
        /// Return a state containing all information that <paramref name="other"/> has that this state doesn't.
        /// May possibly result in an empty state?
        /// </summary>
        public State SubtractFrom(State other)
        {
            State output = Dupe();
            Dictionary<Variable, Instance> differences = new();

            foreach(var pair in other.Subs)
            {
                if (this.Subs.ContainsKey(pair.Key))
                {
                    Instance diff = pair.Value.SubtractInfo(this.Subs[pair.Key]);
                    if (!diff.Equals(Instance.Empty))
                    {
                        differences.Add(pair.Key, diff);
                    }
                }
                else
                {
                    differences.Add(pair.Key, pair.Value);
                }
            }

            output.Subs = differences.ToImmutableDictionary();
            return output;
        }

        public uint Ungroundedness => (uint)this.Subs.Sum(x => x.Value.Ungroundedness);
        public int CompareTo(IGrounded? other) => this.Ungroundedness.CompareTo(other?.Ungroundedness ?? 0);

        public (State, Variable) DeclareVar(string varName)
        {
            State newState = Dupe();
            Variable v = new(ref newState, varName);
            newState.Subs = newState.Subs.Add(v, new Instance.Indefinite());

            return (newState, v);
        }

        //public (State, Variable) DeclareVar<T>(string varName)
        //    where T : Term
        //{
        //    (State s, Variable v) = DeclareVar(varName);
        //    if (s.Subs.TryGetValue(v, out Instance? inst)
        //        && inst is Instance.Indefinite indef)
        //    {
        //        s.Subs = s.Subs.SetItem(v, indef.AddRestriction(x => x is T));
        //    }

        //    return (s, v);
        //}

        public Variable[] DeclareVars(out State result, params string[] varNames)
        {
            result = Dupe();
            List<Variable> newVars = new();

            foreach(string varName in varNames)
            {
                (result, Variable newVar) = result.DeclareVar(varName);
                newVars.Add(newVar);
            }

            return newVars.ToArray();
        }

        public int GenerateVariableID() => ++this.VariableCounter;

        public Term Walk(Term t)
        {
            if (t is Variable v
                && Subs.TryGetValue(v, out Instance? inst)
                && inst is Instance.Definite def)
            {
                return Walk(def.Definition);
            }
            else if (t is Cons c)
            {
                return Cons.Truct(Walk(c.Car), Walk(c.Cdr));
            }

            return t;
        }

        public Term? LookupBySymbol(string symbol)
        {
            return Subs.Where(x => x.Key.Symbol == symbol).FirstOrDefault() is var pair
                && pair.Value is Instance.Definite def
                ? Walk(def.Definition)
                : null;
        }

        public State? Unify(Term u, Term v)
        {
            if (TryUnify(u, v, out State result))
            {
                return result;
            }
            return null;
        }
        public bool TryUnify(Term u, Term v, out State result)
        {
            IO.Debug_Prompt();
            IO.Debug_Print($"Can we unite '{u}' <-> '{v}'?");
            Term alpha = this.Walk(u);
            Term beta = this.Walk(v);

            if (!alpha.Equals(u) || !beta.Equals(v)) IO.Debug_Print($"\ti.e. '{alpha}' <-> '{beta}'");

            if (alpha is Cons alphaCons && beta is Cons betaCons)
            {
                if (TryUnify(alphaCons.Car, betaCons.Car, out State temp))
                {
                    return temp.TryUnify(alphaCons.Cdr, betaCons.Cdr, out result);
                }
            }
            if (alpha is Variable alphaVar)
            {
                return TryExtend(alphaVar, beta, out result);
            }
            else if (beta is Variable betaVar)
            {
                return TryExtend(betaVar, alpha, out result);
            }

            result = this;
            if (alpha.Equals(beta))
            {
                IO.Debug_Print($"'{alpha}' and '{beta}' are the same, so unification is unnecessary");
                return true;
            }
            else
            {
                IO.Debug_Print($"'{alpha}' and '{beta}' are different values, so they can never unify");
                return false;
            }
        }

        private bool TryExtend(Variable v, Term t, out State result)
        {
            if (Subs.TryGetValue(v, out Instance? inst))
            {
                //if this state already has a linked instance for that variable...

                if (inst is Instance.Definite def)
                {
                    //if the variable is already defined,
                    //fail, unless the definitions happen to be equal
                    IO.Debug_Print($"'{v}' is already defined as '{def.Definition}'");

                    result = this;
                    if(def.Definition.Equals(t))
                    {
                        IO.Debug_Print($"\tand '{def.Definition}' and '{t}' CAN unite in {result}");
                        return true;
                    }
                    else
                    {
                        IO.Debug_Print($"\tbut '{def.Definition}' and '{t}' DO NOT unite in {result}");
                        return false;
                    }
                }
                else if (inst is Instance.Indefinite indef)
                {
                    //if the variable is undefined, check to make sure it fits the constraints
                    //complete the extension if everything checks out
                    if (indef.CongruentWith(t))
                    {
                        result = new(this, false);
                        result.Subs = result.Subs.Remove(v).Add(v, indef.BindTo(t));
                        IO.Debug_Print($"'{v}' and '{t}' successfully unite in {result}");
                        return true;
                    }
                    else
                    {
                        result = this;
                        IO.Debug_Print($"'{v}' and '{t}' NEVER unite in {result}");
                        return false;
                    }
                }
            }

            //if we've fallen through to this point, either the state doesn't know the variable
            //or the variable's definition is somehow invalid/null

            throw new InvalidOperationException($"Tried to extend state with variable {v} declared externally");

            //result = this;
            //return false;
        }

        public State? DisUnify(Term u, Term v)
        {
            if (TryDisUnify(u, v, out State result))
            {
                return result;
            }
            return null;
        }
        public bool TryDisUnify(Term u, Term v, out State result)
        {
            //See section 8 of Byrd's paper
            //there are three possible outcomes
            //hypothetical unification of the terms would fail -> disunification succeeds, as the two terms are definitionally not equal
            //hypothetical unification succeeds without extension -> terms already equal, disunification fails
            //hypothetical unification succeeds, but requires extension -> terms may or may not be equal, must be constrained

            IO.Debug_Prompt();
            IO.Debug_Print($"Can we constrain '{u}' != '{v}'?");
            Term alpha = this.Walk(u);
            Term beta = this.Walk(v);

            if (!alpha.Equals(u) || !beta.Equals(v)) IO.Debug_Print($"\ti.e. '{alpha}' != '{beta}'");

            //unification is only possible to start with if one of these terms is a variable
            if (alpha is Variable alphaVar)
            {
                return TryConstrain(alphaVar, new Constraint.Inequality(beta), out result);
            }
            else if (beta is Variable betaVar)
            {
                return TryConstrain(betaVar, new Constraint.Inequality(alpha), out result);
            }

            //we're looking at two concrete terms then, so it comes down to whether they're equal or not
            result = this;
            if(!alpha.Equals(beta))
            {
                IO.Debug_Print($"'{alpha}' and '{beta}' are already different, so no constraint is necessary");
                return true;
            }
            else
            {
                IO.Debug_Print($"'{alpha}' and '{beta}' are the same, so the constraint is impossible");
                return false;
            }
        }

        //public bool TryConstrainType<T>(Term t, out State result)
        //    where T : Term
        //{
        //    IO.Debug_Print($"Is {t} a <{typeof(T)}>?");

        //    if (t is Variable v)
        //    {
        //        return TryConstrain(v, x => x is T, out result);
        //    }
        //    else
        //    {
        //        result = this;
        //        return t is T;
        //    }
        //}

        //public bool TryConstrainNotType<T>(Term t, out State result)
        //    where T : Term
        //{
        //    IO.Debug_Print($"Is {t} not a <{typeof(T)}>?");

        //    if (t is Variable v)
        //    {
        //        return TryConstrain(v, x => x is not T, out result);
        //    }
        //    else
        //    {
        //        result = this;
        //        return t is T;
        //    }
        //}

        private bool TryConstrain(Variable v, Constraint.Inequality pred, out State result)
        {
            if (Subs.TryGetValue(v, out Instance? inst))
            {
                if (inst is Instance.Definite def)
                {
                    //if the variable is already bound to a term, check to see if that term passes the test
                    IO.Debug_Print($"'{v}' is already defined as '{def.Definition}'");
                    result = this;
                    if(pred.Check(def.Definition))
                    {
                        IO.Debug_Print($"\tand '{def.Definition}' satisfies the constraint");
                        return true;
                    }
                    else
                    {
                        IO.Debug_Print($"\tbut '{def.Definition}' VIOLATES the constraint");
                        return false;
                    }
                }
                else if (inst is Instance.Indefinite indef)
                {
                    //otherwise add the new constraint to the list
                    result = new(this, false);
                    result.Subs = result.Subs.Remove(v).Add(v, indef.AddRestriction(pred));
                    IO.Debug_Print($"'{v}' is successfully bound by the new constraint");
                    return true;
                }
            }

            //can't constrain a variable that doesn't exist though
            throw new InvalidOperationException($"Tried to constrain state with variable {v} declared externally");

        }

        public bool TryGetDefinition(string varName, out Term? direct, out Term? ultimate)
        {
            if (this.Subs.Keys.Where(x => x.Symbol == varName).FirstOrDefault() is Variable key
                && key is not null)
            {
                direct = this.Subs[key];
                ultimate = this.Walk(key);
                return true;
            }

            direct = null;
            ultimate = null;
            return false;
        }

        public string GetName() => "State " + this.RecursionLevel + " (" + this.VariableCounter + " var/s)";


        private string DefToString(KeyValuePair<Variable, Instance> pair)
        {
            StringBuilder sb = new();

            sb.Append('\t');
            sb.Append(pair.Key.ToString().PadLeft(this.KeyWidth + 3));
            sb.Append(" => ");
            sb.Append(pair.Value);

            return sb.ToString();
        }

        private string DirectDefToString(KeyValuePair<Variable, Instance> pair)
        {
            StringBuilder sb = new();

            Term result = this.Walk(pair.Key);

            sb.Append('\t');
            sb.Append(pair.Key.ToString().PadLeft(this.KeyWidth + 3));
            sb.Append(" => ");
            sb.Append(result is Variable ? "<Any>" : result.ToString());

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine(this.GetName());

            foreach (var pair in this.Subs.OrderBy(x => x.Key))
            {
                sb.AppendLine(DefToString(pair));
            }

            return sb.ToString();
        }

        public string ToString(int level)
        {
            StringBuilder sb = new();
            sb.AppendLine(this.GetName());

            foreach (var pair in this.Subs.Where(x => x.Key.RecursionLevel <= level).OrderBy(x => x.Key))
            {
                sb.AppendLine(DirectDefToString(pair));
            }

            return sb.ToString();
        }

        public string ToVerboseString() => this.ToString();

        public IEnumerable<string> ToTree() => this.ToTree("", true, false);
        public IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            string connectivePrefix = first ? "" : prefix + IO.BRANCH;
            //string finalItemPrefix = first ? "" : prefix + IO.LEAVES;
            string extraSpacePrefix = first ? "" : prefix + IO.JUMPER;

            var items = this.Subs.OrderBy(x => x.Key);

            yield return connectivePrefix + (items.Any() ? IO.HEADER : IO.ALONER) +
                "State " + this.RecursionLevel + " (" + this.VariableCounter + " var/s):";


            foreach(var pair in items.SkipLast(1))
            {
                foreach(string lines in BranchHelper(extraSpacePrefix, pair, false, false))
                {
                    yield return lines;
                }
            }

            foreach (string lines in BranchHelper(extraSpacePrefix, items.Last(), false, true))
            {
                yield return lines;
            }
        }

        private static IEnumerable<string> BranchHelper(string prefix, KeyValuePair<Variable, Instance> pair, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + IO.BRANCH;
            string childPrefix = first ? "" : prefix + IO.JUMPER;

            yield return parentPrefix + IO.HEADER + pair.Key.ToString();

            if (pair.Value.Equals(null))
            {
                yield return "NULL";
            }
            else
            {
                foreach(string lines in pair.Value.ToTree(childPrefix, false, true))
                {
                    yield return lines;
                }
            }
        }
    }
}
