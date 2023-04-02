using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public class Variable : Term, IComparable<Variable>
    {
        public readonly string Symbol;

        private readonly int ID;
        public readonly int RecursionLevel;

        public Variable(State s, string symbol)
        {
            this.Symbol = symbol;
            this.ID = s.GenerateVariableID();
            this.RecursionLevel = s.RecursionLevel;
        }

        public override Term Dereference(State s)
        {
            if (s.Subs.TryGetValue(this, out Term? lookup) && lookup is not null)
            {
                return lookup.Dereference(s);
            }
            return this;
        }

        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs(State s, Variable other) => other.ID == this.ID;

        public override State? UnifyWith(State s, Term other) => other.UnifyWith(s, this);
        public override State? UnifyWith(State s, Variable other) => other.SameAs(s, this) ? s.Affirm(other, this) : s.Extend(other, this);
        public override State? UnifyWith<D>(State s, Variable<D> other) => s.Extend(this, other);
        public override State? UnifyWith<D>(State s, Value<D> other) => s.Extend(this, other);
        public override State? UnifyWith(State s, Nil other) => s.Extend(this, other);
        public override State? UnifyWith(State s, Cons other) => s.Extend(this, other);


        public override string ToString() => this.Symbol + "#" + this.RecursionLevel;
        public override IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            yield return prefix + IO.BRANCH + this.ToString();
        }

        public override int GetHashCode() => this.ID;

        public int CompareTo(Variable? other)
        {
            if (other is null)
            {
                return 1; //apparently everything is greater-than Null
            }

            if (this.RecursionLevel != other.RecursionLevel)
            {
                return this.RecursionLevel.CompareTo(other.RecursionLevel);
            }
            else
            {
                return this.ID.CompareTo(other.ID);
            }
        }
    }

    public class Variable<D> : Variable
    {

        public Variable(State s, string symbol) : base(s, symbol) { }


        public R Dispatch<O, R>(Func<Value<D>, R> valueCase, Func<Variable<D>, R> variableCase)
        {
            return variableCase(this);
        }
        public Term Reflect() => this;
        public bool TryReify(out D? result)
        {
            result = default;
            return false;
        }

        public override State? UnifyWith(State s, Term other) => s.Reject(other, this);
        public override State? UnifyWith<O>(State s, Variable<O> other) => other.SameAs(s, this) ? s.Affirm(other, this) : s.Extend(other, this);

        public override string ToString() => "<" + typeof(D).Name + "> " + base.ToString();
    }
}
