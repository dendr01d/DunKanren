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
    public class State : IPrintable
    {
        private static int _globalVariableCount = 0;

        public ImmutableDictionary<ADT.Term.Variable, VariableDefinition.> Subs;
        public int VariableCount { get => Subs.Count; }
        public int RecursionLevel { get; private set; }

        private int _symbolFieldWidth => this.Subs.Max(x => x.Symbol.Length);

        private State()
        {
            Subs = ImmutableHashSet.Create<ADT.Term.Variable>();
            RecursionLevel = 0;
        }

        private State(State s, bool recursing) : this()
        {
            Subs = s.Subs.ToImmutableHashSet();

            this.RecursionLevel = recursing ? s.RecursionLevel + 1 : s.RecursionLevel;
        }

        public static State InitialState() => new();

        public State Next() => new(this, true);

        public State Dupe() => new(this, false);

        public ADT.Term.Variable.Free<T> DeclareVariable<T>(string symbol, out State output)
            where T : ADT.Term
        {
            output = Dupe();
            ADT.Term.Variable.Free<T> newVar = new ADT.Term.Variable.Free<T>(symbol);
            output.Subs = output.Subs.Add(newVar);
            return newVar;
        }

        public ADT.Term.Variable.Bound<T> DefineVariable<T>(string symbol, T value, out State output)
            where T : ADT.Term
        {
            output = Dupe();
            ADT.Term.Variable.Bound<T> newVar = new ADT.Term.Variable.Bound<T>(symbol, value);
            output.Subs = output.Subs.Add(newVar);
            return newVar;
        }

        private State? TryBindVariable<T>(ADT.Term.Variable.Free<T> var, T value)
            where T : ADT.Term
        {
            State? output = null;

            if (Subs.Contains(var)
                && var.TryBind(value) is ADT.Term.Variable.Bound<T> bound
                && bound != null)
            {
                output = Dupe();
                output.Subs = output.Subs.Remove(var).Add(bound);
            }

            return output;
        }

        public static int GenerateVariableID() => _globalVariableCount++;

        public ADT.Term? Walk(ADT.Term.Variable v)
        {
            if (v.DereferenceVia(this) is ADT.Term def
                && def != null)
            {
                return def is ADT.Term.Variable nestedVar ? Walk(nestedVar) : def;
            }

            return def;
        }

        public ADT.Term Reify(ADT.Term.Variable original, ref Predicate<ADT.Term> constraints)
        {
            if (Subs.get(original, out VariableDefinition? vd)
                && vd is VariableDefinition.Negative vdn)
            {
                if (this.Negs.TryGetValue(v, out var negs))
                {
                    constraints.AddRange(negs);
                }

                if (this.Subs.TryGetValue(v, out Term? t) && !t.Equals(null) && !t.Equals(original))
                {
                    return Reify(t, ref constraints);
                }
            }

            return original;
        }

        public ADT.Term? LookupBySymbol(string symbol)
        {
            var vdp = Subs.Where(x => x.Key.Symbol == symbol).FirstOrDefault().Value as VariableDefinition.Positive;
            return vdp?.Definition;
        }

        public bool TryUnify(Term u, Term v, out State result)
        {
            IO.Debug_Print(u.ToString() + " EQ? " + v.ToString());
            Term alpha = this.Walk(u);
            Term beta = this.Walk(v);

            if (!alpha.SameAs(this, u) || !beta.SameAs(this, v)) IO.Debug_Print("===> " + alpha.ToString() + " EQ? " + beta.ToString());

            return alpha.TryUnifyWith(this, beta, out result);
        }

        public bool TryExtend(Variable v, Term t, out State result)
        {
            if (this.Negs.TryGetValue(v, out var ns) && ns.Contains(t))
            {
                //if the new substitution violates a constraint, fail right off the bat
                IO.Debug_Print(v.ToString() + " NEV " + t.ToString());
                result = this;
                return false;
            }
            else if (this.Subs.TryGetValue(v, out var ss2) && ss2.Equals(t))
            {
                //if the terms are already equal without any new substitutions, succeed
                result = this;
                return VerifyConstraints(ref result);
            }
            else if (!this.Subs.ContainsKey(v) || (this.Subs.TryGetValue(v, out var ss) && ss.Equals(v)))
            {

                result = new(this, false);
                result.Subs[v] = t;

                if (!this.Subs.ContainsKey(v))
                {
                    result.VariableCounter++;
                }

                IO.Debug_Print(v.ToString() + " DEF " + t + " in " + result.ToString());
                return VerifyConstraints(ref result);
            }
            else //should only occur if it's attempting to redefine an existing variable binding?
            {
                //throw new InvalidOperationException();
                result = this;
                return false;
            }
        }

        private static bool VerifyConstraints(ref State result)
        {
            foreach (var kvp in result.Negs)
            {
                Variable lastKey = kvp.Key;

                while (lastKey.Dereference(result) is Variable t && !t.Equals(lastKey))
                {
                    if (result.Subs.TryGetValue(lastKey, out Term? tDef)
                        && !ReferenceEquals(tDef, null)
                        && kvp.Value.Contains(tDef))
                    {
                        result = State.InitialState();
                        return false;
                    }

                    lastKey = t;
                }
            }

            return true;
        }

        public bool Affirm(Term u, Term v, out State result)
        {
            result = this;
            IO.Debug_Print(u.ToString() + " EQL " + v.ToString() + " in " + result.ToString());
            return true;
        }

        public bool Reject(Term u, Term v, out State result)
        {
            result = this;
            IO.Debug_Print(u.ToString() + " NEQ " + v.ToString() + " in " + result.ToString());
            return false;
        }


        public bool TryDisUnify(Term u, Term v, out State result)
        {
            //See section 8 of Byrd's paper

            IO.Debug_Print(u.ToString() + " NQ? " + v.ToString());

            if (!this.TryUnify(u, v, out State unifiedResult))
            {
                //if unification fails, then the two terms can never be equal
                //and so we don't need to keep a constraint around to show it
                IO.Debug_Print("===>\n" + u.ToString() + " NEV " + v.ToString() + "\n");
                result = this;
                return true;
            }
            else if (unifiedResult == this)
            {
                //if unification succeeds without extending the resulting state
                //then the terms are ALREADY equal, and we've failed outright
                IO.Debug_Print("===>\n" + u.ToString() + " EQL " + v.ToString() + "\n");
                result = this;
                return false;
            }
            else
            {
                //if unification succeeds, but requires new subs TO succeed
                //then we simply disallow any of those subs from ever being made
                result = ConstrainValues(this, DifferentiateSubs(this, unifiedResult));
                IO.Debug_Print("===>\n" + u.ToString() + " NDF " + v.ToString() + " in " + result.ToString());
                return true;
            }
        }

        private static Dictionary<Variable, Term> DifferentiateSubs(State oldState, State newState)
        {
            return new Dictionary<Variable, Term>(newState.Subs.Where(
                x => 
                !oldState.Subs.ContainsKey(x.Key) || //new key inserted
                (oldState.Subs[x.Key].Equals(x.Key) && !newState.Subs[x.Key].Equals(x.Key)) || //frshly-declared var redefined
                ((!oldState.Subs[x.Key]?.Equals(newState.Subs[x.Key])) ?? false)) //defined var redefined?
                .Where(x => x.Value is not null)); //don't bother if so
        }

        private static State ConstrainValues(State prev, Dictionary<Variable, Term> newNegs)
        {
            State output = prev.Dupe();

            foreach(var pair in newNegs)
            {
                if (!output.Negs.ContainsKey(pair.Key))
                {
                    output.Negs.Add(pair.Key, new HashSet<Term>());
                }

                output.Negs[pair.Key].Add(pair.Value);
            }

            return output;
        }

        public bool TryGetDefinition(string varName, out Term? direct, out Term? ultimate)
        {
            if (this.Subs.Keys.Where(x => x.Symbol == varName).FirstOrDefault() is Variable key && !key.Equals(null))
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


        private string DefToString(KeyValuePair<Variable, Term> pair)
        {
            StringBuilder sb = new();

            List<Term> constraints = new();
            Term reified = this.Reify(pair.Key, ref constraints);

            sb.Append('\t');
            sb.Append(pair.Key.ToString().PadLeft(this._symbolFieldWidth + 3));
            sb.Append(" => ");
            sb.Append(pair.Value);
            if (!reified.Equals(pair.Value)) sb.Append($" (-> {reified.ToString()})");
            if (constraints.Any()) sb.Append($" ¬({String.Join(", ", constraints.Select(x => x.ToString()))})");

            return sb.ToString();
        }

        private string DirectDefToString(KeyValuePair<Variable, Term> pair)
        {
            StringBuilder sb = new();

            List<Term> constraints = new();
            this.Reify(pair.Key, ref constraints);

            Term result = pair.Key.Dereference(this);

            sb.Append('\t');
            sb.Append(pair.Key.ToString().PadLeft(this._symbolFieldWidth + 3));
            sb.Append(" => ");
            sb.Append(result is Variable ? "<Any>" : result.ToString());
            if (constraints.Any()) sb.Append($" ¬({String.Join(", ", constraints.Select(x => x.ToString()))})");

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

        private static IEnumerable<string> BranchHelper(string prefix, KeyValuePair<Variable, Term> pair, bool first, bool last)
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
