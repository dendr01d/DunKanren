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

            s.Subs = s.Subs.Add(this, new Instance.Indefinite());
        }

        protected Variable(Variable original)
        {
            this.Symbol = original.Symbol;
            this.ID = original.ID;
            this.RecursionLevel = original.RecursionLevel;
        }

        public override uint Ungroundedness => 1;

        public override Term Dereference(State s)
        {
            if (s.Subs.TryGetValue(this, out Instance? lookup)
                && lookup is Instance.Definite def
                && def.Definition is Variable newVar
                && !newVar.Equals(this))
            {
                return lookup.Dereference(s);
            }
            return this;
        }

        public override bool TermEquals(State s, Term other) => other.TermEquals(s, this);
        public override bool TermEquals(State s, Variable other) => other.ID == this.ID;

        public override bool TryUnifyWith(State s, Term other, out State result) => other.TryUnifyWith(s, this, out result);
        public override bool TryUnifyWith(State s, Variable other, out State result) =>
            other.TermEquals(s, this)
            ? s.Affirm(other, this, out result)
            : s.TryExtend(other, this, out result);
        public override bool TryUnifyWith<D>(State s, Variable<D> other, out State result) => other.TryUnifyWith(s, this, out result);

        public override bool TryUnifyWith<T>(State s, IUnifiable<T> other, out State result)
        {
            return s.TryExtend(new Variable<T>(this), other.Identity, out result);
        }

        public override string ToString() => this.Symbol + "#" + this.RecursionLevel;

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

    /// <summary>
    /// A variable that's naturally constrained such that it can only unify to its desired type,
    /// or other similarly-typed variables
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Variable<T> : Variable, IUnifiable<T>
        where T : Term
    {
        public Variable(ref State s, string symbol) : base(ref s, symbol) { }
        public Variable(Variable original) : base(original) { }

        public override bool TryUnifyWith(State s, Term other, out State result) => other.TryUnifyWith(s, this, out result);

        public bool TryUnifyWith(State s, T other, out State result) => s.TryExtend(this, other, out result);

        public bool TryUnifyWith(State s, Variable<T> other, out State result) =>
            other.TermEquals(s, this)
            ? s.Affirm(other, this, out result)
            : s.Reject(other, this, out result);

        public bool TryUnifyWith(State s, IUnifiable<T> other, out State result) => s.TryExtend(this, other.Identity, out result);
    }

    public interface IUnifiable<T> where T : Term
    {
        public Term Identity { get; }

        public bool TryUnifyWith(State s, T other, out State result);
        public bool TryUnifyWith(State s, Variable<T> other, out State result);
        public bool TryUnifyWith(State s, IUnifiable<T> other, out State result);
    }
}
