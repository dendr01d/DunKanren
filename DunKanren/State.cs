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
            ADT.Term.Variable[] newVars = varNames.Select(x => new ADT.Term.Variable(x, newState)).ToArray();
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

        public ADT.Term? LookupBySymbol(string symbol)
        {
            return _subs.Keys.FirstOrDefault(x => x.Symbol == symbol) is ADT.Term.Variable v ? LookupByVariable(v) : null;
        }

        public ADT.Term? LookupByVariable(ADT.Term.Variable vari)
        {
            return _subs.GetValueOrDefault(vari) is Binding.Bound bound ? bound.GetValue() : null;
        }

        //public bool TryUnify(ADT.Term t1, ADT.Term t2, out State result)
        //{
        //    IO.Debug_Print(t1.ToString() + " EQ? " + t2.ToString());
        //    ADT.Term alpha = t1 is ADT.Term.Variable v1 ? this.Walk(v1) ?? t1 : t1;
        //    ADT.Term beta = t2 is ADT.Term.Variable v2 ? this.Walk(v2) ?? t2 : t2;

        //    if (!alpha.Equals(this, t1) || !beta.Equals(this, t2)) IO.Debug_Print("===> " + alpha.ToString() + " EQ? " + beta.ToString());

        //    return alpha.TryUnifyWith(this, beta, out result);
        //}

        public State? Extend(ADT.Term.Variable v, ADT.Term t)
        {
            if (_subs.TryGetValue(v, out Binding? b))
            {
                if (b is Binding.Free bf)
                {
                    //TODO I'm forseeing a problem here where a variable is bound to another variable,
                    //but the restrictions on the associated bindings don't match up?

                    if (bf.CanBind(t))
                    {
                        State output = Dupe();
                        output._subs = output._subs.Remove(v).Add(v, bf.Bind(t));
                        return output;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    throw new Exception($"Tried to extend state by binding bound or null variable {v}");
                }
            }
            else
            {
                State output = Dupe();
                output._subs = output._subs.Add(v, Binding.Bound.Wrap(t));
                return output;
            }
        }

        public State? Restrict(ADT.Term.Variable v, Predicate<ADT.Term> restriction)
        {
            //if the variable is already bound, and the term it's bound to fails
            //then the entire state fails
            //if the variable is already bound, and the restriction passes, then great
            //(we don't NEED to add the restriction in this case, but I will for the sake of auditing)
            //if the variable is NOT bound, just add the restriction

            if (_subs.TryGetValue(v, out Binding? b))
            {
                if (b is Binding.Bound bb
                    && !restriction(bb.GetValue()))
                {
                    return null;
                }
                else if (b is Binding.Free bf)
                {
                    State output = Dupe();
                    output._subs = output._subs.Remove(v).Add(v, bf.AddRestriction(restriction));
                    return output;
                }
            }
            else
            {
                State output = Dupe();
                output._subs = output._subs.Add(v, new Binding.Free.Untyped(restriction));
                return output;
            }

            //??? why does this need to be here
            return null;
        }

        public bool TryGetDefinition(string varName, out ADT.Term? direct, out ADT.Term? ultimate)
        {
            if (this._subs.Keys.Where(x => x.Symbol == varName).FirstOrDefault() is ADT.Term.Variable key
                && !key.Equals(null))
            {
                direct = this._subs[key] is Binding.Bound bb ? bb.GetValue() : null;
                ultimate = this.Walk(key);
                return true;
            }

            direct = null;
            ultimate = null;
            return false;
        }

        public string GetName() => "State " + this.RecursionLevel + " (" + this._variableCounter + " var/s)";


        private string DefToString(KeyValuePair<ADT.Term.Variable, Binding> pair)
        {
            StringBuilder sb = new();

            ADT.Term reified = pair.Key.Reify(this);

            sb.Append('\t');
            sb.Append(pair.Key.ToString().PadLeft(this._symbolFieldWidth + 3));
            sb.Append(" => ");
            sb.Append(pair.Value);
            if (!reified.Equals(pair.Value)) sb.Append($" (-> {reified.ToString()})");

            return sb.ToString();
        }

        private string DirectDefToString(KeyValuePair<ADT.Term.Variable, Binding> pair)
        {
            StringBuilder sb = new();

            ADT.Term reified = pair.Key.Reify(this);

            ADT.Term? result = Walk(pair.Key);

            sb.Append('\t');
            sb.Append(pair.Key.ToString().PadLeft(this._symbolFieldWidth + 3));
            sb.Append(" => ");
            sb.Append(result is ADT.Term.Variable ? "<Any>" : result.ToString());

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine(this.GetName());

            foreach (var pair in this._subs.OrderBy(x => x.Key))
            {
                sb.AppendLine(DefToString(pair));
            }

            return sb.ToString();
        }

        public string ToString(int level)
        {
            StringBuilder sb = new();
            sb.AppendLine(this.GetName());

            foreach (var pair in this._subs.Where(x => x.Key.RecursionLevel <= level).OrderBy(x => x.Key))
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

            var items = _subs.OrderBy(x => x.Key);

            yield return connectivePrefix + (items.Any() ? IO.HEADER : IO.ALONER) +
                "State " + this.RecursionLevel + " (" + this._variableCounter + " var/s):";


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

        private static IEnumerable<string> BranchHelper(string prefix, KeyValuePair<ADT.Term.Variable, Binding> pair, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + IO.BRANCH;
            string childPrefix = first ? "" : prefix + IO.JUMPER;

            yield return parentPrefix + IO.HEADER + pair.Key.ToString();

            if (pair.Value.Equals(null))
            {
                yield return "NULL";
            }
            else if (pair.Value is Binding.Bound bb)
            {
                foreach(string lines in bb.GetValue().ToTree(childPrefix, false, true))
                {
                    yield return lines;
                }
            }
        }
    }
}
