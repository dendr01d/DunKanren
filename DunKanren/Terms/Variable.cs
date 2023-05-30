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

        public Variable(ref State s, string symbol)
        {
            this.Symbol = symbol;
            this.ID = s.GenerateVariableID();
            this.RecursionLevel = s.RecursionLevel;

            s.Subs.Add(this, this);
        }

        public override int Ungroundedness => 1;

        public override Term Dereference(State s)
        {
            if (s.Subs.TryGetValue(this, out Term? lookup) && lookup is not null && !lookup.Equals(this))
            {
                return lookup.Dereference(s);
            }
            return this;
        }

        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs(State s, Variable other) => other.ID == this.ID;

        public override bool TryUnifyWith(State s, Term other, out State result) => other.TryUnifyWith(s, this, out result);
        public override bool TryUnifyWith(State s, Variable other, out State result) =>
            other.SameAs(s, this)
            ? s.Affirm(other, this, out result)
            : s.TryExtend(other, this, out result);
        public override bool TryUnifyWith<D>(State s, Variable<D> other, out State result) => s.TryExtend(this, other, out result);
        public override bool TryUnifyWith<D>(State s, Value<D> other, out State result) => s.TryExtend(this, other, out result);
        public override bool TryUnifyWith(State s, Nil other, out State result) => s.TryExtend(this, other, out result);
        public override bool TryUnifyWith(State s, LCons other, out State result) => s.TryExtend(this, other, out result);
        public override bool TryUnifyWith<D1, D2>(State s, Cons<D1, D2> other, out State result)
            => s.TryExtend(this, other, out result);

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

        public Variable(ref State s, string symbol) : base(ref s, symbol) { }


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

        public override bool TryUnifyWith(State s, Term other, out State result) => s.Reject(other, this, out result);
        public override bool TryUnifyWith<O>(State s, Variable<O> other, out State result) =>
            other.SameAs(s, this)
            ? s.Affirm(other, this, out result)
            : s.TryExtend(other, this, out result);

        public override string ToString() => "<" + typeof(D).Name + "> " + base.ToString();
    }
}
