using System;
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

        public uint Ungroundedness => (uint)this.Subs.Sum(x => x.Value.Ungroundedness);
        public int CompareTo(IGrounded? other) => this.Ungroundedness.CompareTo(other?.Ungroundedness ?? 0);

        public (State, Variable) DeclareVar(string varName)
        {
            State newState = Dupe();
            Variable v = new(ref newState, varName);
            newState.Subs = newState.Subs.Add(v, new Instance.Indefinite());

            return (newState, v);
        }

        public (State, Variable) DeclareVar<T>(string varName)
            where T : Term
        {
            (State s, Variable v) = DeclareVar(varName);
            if (s.Subs.TryGetValue(v, out Instance? inst)
                && inst is Instance.Indefinite indef)
            {
                s.Subs = s.Subs.SetItem(v, indef.AddRestriction(x => x is T));
            }

            return (s, v);
        }

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
                if (def.Definition is Variable v2)
                {
                    return Walk(v2);
                }
                else
                {
                    return def.Definition;
                }
            }
            else if (t is Cons c)
            {
                return Cons.Truct(Walk(c.Car), Walk(c.Cdr));
            }

            return t;
        }

        public Term? LookupBySymbol(string symbol)
        {
            return this.Subs.Where(x => x.Key.Symbol == symbol).FirstOrDefault().Value;
        }

        public bool TryUnify(Term u, Term v, out State result)
        {
            IO.Debug_Print(u.ToString() + " EQ? " + v.ToString());
            Term alpha = this.Walk(u);
            Term beta = this.Walk(v);

            if (!alpha.TermEquals(this, u) || !beta.TermEquals(this, v)) IO.Debug_Print("===> " + alpha.ToString() + " EQ? " + beta.ToString());

            if (alpha is Variable alphaVar)
            {
                return TryExtend(alphaVar, beta, out result);
            }
            else if (beta is Variable betaVar)
            {
                return TryExtend(betaVar, alpha, out result);
            }
            else
            {
                result = this;
                return alpha.TermEquals(this, beta);
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
                    result = this;
                    if(def.Definition.TermEquals(this, t))
                    {
                        IO.Debug_Print(v.ToString() + " EQL " + t.ToString() + " in " + result.ToString());
                        return true;
                    }
                    else
                    {
                        IO.Debug_Print(v.ToString() + " NEQ " + t.ToString() + " in " + result.ToString());
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
                        IO.Debug_Print(v.ToString() + " DEF " + t.ToString() + " in " + result.ToString());
                        return true;
                    }
                    else
                    {
                        result = this;
                        IO.Debug_Print(v.ToString() + " NEV " + t.ToString() + " in " + result.ToString());
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

        public bool TryDisUnify(Term u, Term v, out State result)
        {
            //See section 8 of Byrd's paper
            //there are three possible outcomes
            //hypothetical unification of the terms would fail -> disunification succeeds, as the two terms are definitionally not equal
            //hypothetical unification succeeds without extension -> terms already equal, disunification fails
            //hypothetical unification succeeds, but requires extension -> terms may or may not be equal, must be constrained

            IO.Debug_Print(u.ToString() + " NQ? " + v.ToString());

            //unification is only possible to start with if one of these terms is a variable
            if (u is Variable uV)
            {
                return TryConstrain(uV, x => !x.TermEquals(this, v), out result);
            }
            else if (v is Variable vV)
            {
                return TryConstrain(vV, x => !x.TermEquals(this, u), out result);
            }

            //we're looking at two concrete terms then, so it comes down to whether they're equal or not
            result = this;
            return !u.TermEquals(this, v);
        }

        private bool TryConstrain(Variable v, Predicate<Term> pred, out State result)
        {
            if (Subs.TryGetValue(v, out Instance? inst))
            {
                if (inst is Instance.Definite def)
                {
                    //if the variable is already bound to a term, check to see if that term passes the test
                    result = this;
                    return pred(def.Definition);
                }
                else if (inst is Instance.Indefinite indef)
                {
                    //otherwise add the new constraint to the list
                    result = new(this, false);
                    result.Subs = result.Subs.Remove(v).Add(v, indef.AddRestriction(pred));
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
