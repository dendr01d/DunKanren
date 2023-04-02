using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public class State : IPrintable
    {
        public readonly Dictionary<Variable, Term?> Subs;
        public readonly Dictionary<Variable, List<Term?>> Negs;

        public readonly int RecursionLevel;
        private int VariableCounter;

        
        private State()
        {
            this.Subs = new Dictionary<Variable, Term?>();
            this.Negs = new Dictionary<Variable, List<Term?>>();
            this.VariableCounter = 0;
            this.RecursionLevel = 0;
        }

        private State(State s, bool recursing)
        {
            this.Subs = new Dictionary<Variable, Term?>(s.Subs);
            this.Negs = s.Negs.ToDictionary(x => x.Key, x => x.Value.ToList());

            this.VariableCounter = s.VariableCounter;
            this.RecursionLevel = recursing ? s.RecursionLevel + 1 : s.RecursionLevel;
        }

        public static State InitialState() => new();

        public State Next() => new(this, true);

        public State Dupe() => new State(this, false);


        public Variable[]InstantiateVars(params string[] symbols)
        {
            return symbols.Select(x => new Variable(this, x)).ToArray();
        }

        public State DeclareVars(params Variable[] vars)
        {
            State output = this.Dupe();
            foreach(Variable v in vars)
            {
                output = new State(output, false);
                output.Subs[v] = null;
            }
            return output;
        }

        /*
        public State DeclareVars<T>(params Variable<T>[] vars)
        {
            State output = this.Dupe();
            foreach (Variable v in vars)
            {
                output = output.Extend(v, null);
            }
            return output;
        }
        */

        public int GenerateVariableID() => ++this.VariableCounter;

        public Term Walk(Term t)
        {
            return t.Dereference(this);
        }

        public State? Unify(Term u, Term v)
        {
            IO.Debug_Print(u.ToString() + " ?= " + v.ToString());
            Term alpha = this.Walk(u);
            Term beta = this.Walk(v);

            if (!alpha.SameAs(this, u) || !beta.SameAs(this, v)) IO.Debug_Print(alpha.ToString() + " ?= " + beta.ToString());
            return alpha.UnifyWith(this, beta);
        }

        public State? Extend(Variable v, Term t)
        {
            if (this.Negs.TryGetValue(v, out List<Term?>? nl) && nl.Contains(t))
            {
                IO.Debug_Print(v.ToString() + " ~~> ¬" + t.ToString());
                return Reject(v, t);
            }

            State output = new(this, false);
            output.Subs[v] = t;
            
            if (t is not null ) IO.Debug_Print(v.ToString() + " := " + t! + " in " + output.ToString());

            return output;
        }

        public State Affirm(Term u, Term v)
        {
            IO.Debug_Print(u.ToString() + " == " + v.ToString() + " in " + this.ToString());
            return this;
        }

        public State? Reject(Term u, Term v)
        {
            IO.Debug_Print(u.ToString() + " <> " + v.ToString() + " in " + this.ToString());
            return null;
        }


        public State? DisUnify(Term u, Term v)
        {
            //See section 8 of Byrd's paper

            IO.Debug_Print(u.ToString() + " ?!= " + v.ToString());
            State? result = this.Unify(u, v);

            if (result is null)
            {
                IO.Debug_Print("===>\n" + u.ToString() + " != " + v.ToString() + "\n");
                return this;
            }
            else if (result == this)
            {
                IO.Debug_Print("===>\n" + u.ToString() + " !!= " + v.ToString() + "\n");
                return null;
            }
            else
            {
                //find what new associations were created and turn them into constraints
                result = ConstrainValues(this, DifferentiateSubs(this, result));
                IO.Debug_Print("===>\n" + u.ToString() + " !<> " + v.ToString() + " in " + result.ToString());
                return result;
            }
        }

        private static Dictionary<Variable, Term?> DifferentiateSubs(State oldState, State newState)
        {
            return new Dictionary<Variable, Term?>(newState.Subs.Where(
                x => 
                !oldState.Subs.ContainsKey(x.Key) || //new key inserted
                (oldState.Subs[x.Key] is null && newState.Subs[x.Key] is not null) || //declared var redefined
                ((!oldState.Subs[x.Key]?.Equals(newState.Subs[x.Key])) ?? false)) //defined var redefined?
                .Where(x => x.Value is not null)); //don't bother if so
        }

        private static State ConstrainValues(State prev, Dictionary<Variable, Term?> newNegs)
        {
            State output = prev.Dupe();

            foreach(var pair in newNegs)
            {
                if (!output.Negs.TryGetValue(pair.Key, out List<Term?>? nl))
                {
                    output.Negs[pair.Key] = new List<Term?>();
                }
                
                if (!output.Negs[pair.Key].Contains(pair.Value))
                {
                    output.Negs[pair.Key].Add(pair.Value);
                }
            }

            return output;
        }


        public string GetName() => "State " + this.RecursionLevel + " (" + this.VariableCounter + " var/s)";

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.GetName());

            foreach(var pair in this.Subs.OrderBy(x => x.Key))
            {
                sb.Append('\t');
                sb.Append(pair.Key.ToString());
                sb.Append(" => ");
                sb.Append(pair.Value?.Dereference(this).ToString() ?? "()");
                if (this.Negs.TryGetValue(pair.Key, out List<Term?>? nots))
                {
                    sb.Append(" ~~> ¬(");
                    sb.Append(String.Join(", ", nots));
                    sb.Append(')');
                }
                sb.AppendLine("; ");
            }

            return sb.ToString();
        }

        public string ToString(int level)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.GetName());

            foreach (var pair in this.Subs.Where(x => x.Key.RecursionLevel == level).OrderBy(x => x.Key))
            {
                sb.Append('\t');
                sb.Append(pair.Key.ToString());
                sb.Append(" => ");
                sb.Append(pair.Value?.Dereference(this).ToString() ?? "Any_");
                if (this.Negs.TryGetValue(pair.Key, out List<Term?>? nots))
                {
                    sb.Append(" ¬(");
                    sb.Append(String.Join(", ", nots));
                    sb.Append(')');
                }
                sb.AppendLine("; ");
            }

            return sb.ToString();
        }

        public string ToVerboseString() => this.ToString();

        public IEnumerable<string> ToTree() => this.ToTree("", true, false);
        public IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + IO.BRANCH;
            string childPrefix = first ? "" : prefix + IO.JUMPER;

            var items = this.Subs.OrderBy(x => x.Key);

            yield return parentPrefix + (items.Any() ? IO.HEADER : IO.ALONER) +
                "State " + this.RecursionLevel + " (" + this.VariableCounter + " var/s):";


            foreach(var pair in items.SkipLast(1))
            {
                foreach(string lines in BranchHelper(childPrefix, pair, false, false))
                {
                    yield return lines;
                }
            }

            foreach (string lines in BranchHelper(childPrefix, items.Last(), false, true))
            {
                yield return lines;
            }
        }

        private static IEnumerable<string> BranchHelper(string prefix, KeyValuePair<Variable, Term?> pair, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + IO.BRANCH;
            string childPrefix = first ? "" : prefix + IO.JUMPER;

            yield return parentPrefix + IO.HEADER + pair.Key.ToString();

            if (pair.Value is null)
            {
                yield return "NULL";
            }
            else
            {
                foreach(string lines in pair.Value!.ToTree(childPrefix, false, true))
                {
                    yield return lines;
                }
            }
        }
    }
}
