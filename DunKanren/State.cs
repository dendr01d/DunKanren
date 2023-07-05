using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

using System.Collections.Immutable;

namespace DunKanren
{
    public class State : IPrintable
    {
        private int _variableCounter = 0;

        private ImmutableDictionary<ADT.Term.Variable, Binding> _subs;
        public int VariableCount { get => _subs.Count; }
        public int RecursionLevel { get; private set; }

        private int _symbolFieldWidth => this._subs.Keys.Max(x => x.Symbol.Length);

        private State()
        {
            _subs = ImmutableDictionary.Create<ADT.Term.Variable, Binding>();
            RecursionLevel = 0;
        }

        private State(State s, bool recursing) : this()
        {
            _subs = s._subs.ToImmutableDictionary(x => x.Key, x => x.Value.ToNew());

            this.RecursionLevel = recursing ? s.RecursionLevel + 1 : s.RecursionLevel;
        }

        public static State InitialState() => new();

        public State Next() => new(this, true);

        public State Dupe() => new(this, false);

        public ADT.Term.Variable[] DeclareVars(out State result, params string[] varNames)
        {
            State newState = Dupe();
            ADT.Term.Variable[] newVars = varNames.Select(x => new ADT.Term.Variable(newState, x)).ToArray();
            newState._subs = newState._subs.AddRange(newVars.Select(x => new KeyValuePair<ADT.Term.Variable, Binding>(x, new Binding.Free.Untyped())));

            result = newState;
            return newVars;
        }

        public int GenerateVariableID() => ++this._variableCounter;

        public ADT.Term? Walk(ADT.Term.Variable v)
        {
            if (_subs.GetValueOrDefault(v) is Binding.Bound def
                && def.GetValue() is ADT.Term t)
            {
                return t is ADT.Term.Variable v2
                    ? Walk(v2)
                    : t;
            }
            return null;
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
            return _subs.Keys.FirstOrDefault(x => x.Symbol == symbol) is ADT.Term.Variable v ? LookupByVariable(v) : null;
        }

        public ADT.Term? LookupByVariable(ADT.Term.Variable vari)
        {
            return _subs.GetValueOrDefault(vari) is Binding.Bound bound ? bound.GetValue() : null;
        }

        public bool TryUnify(ADT.Term t1, ADT.Term t2, out State result)
        {
            IO.Debug_Print(t1.ToString() + " EQ? " + t2.ToString());
            ADT.Term alpha = t1 is ADT.Term.Variable v1 ? this.Walk(v1) ?? t1 : t1;
            ADT.Term beta = t2 is ADT.Term.Variable v2 ? this.Walk(v2) ?? t2 : t2;

            if (!alpha.Equals(this, t1) || !beta.Equals(this, t2)) IO.Debug_Print("===> " + alpha.ToString() + " EQ? " + beta.ToString());

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


        public bool TryDisUnify(ADT.Term.Variable v, ADT.Term t, out State result)
        {
            //See section 8 of Byrd's paper

            IO.Debug_Print(v.ToString() + " NQ? " + t.ToString());

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
