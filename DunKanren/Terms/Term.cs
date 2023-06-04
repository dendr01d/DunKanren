using DunKanren.Terms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public abstract class Term : IPrintable, IGrounded
    {
        public static readonly Value<bool> True = new(true);
        public static readonly Value<bool> False = new(false);

        public static readonly Nil NIL = new();

        public virtual Term Dereference(State s) => this;

        public Term Identity => this;

        #region Equivalence
        public abstract bool SameAs(State s, Term other);

        public virtual bool SameAs(State s, Variable other) => false;
        public virtual bool SameAs<D>(State s, Variable<D> other) where D : Term => false;
        public virtual bool SameAs<D>(State s, Value<D> other) => false;
        public virtual bool SameAs(State s, Number other) => false;
        public virtual bool SameAs(State s, Nil other) => false;
        public virtual bool SameAs(State s, LCons other) => false;
        public virtual bool SameAs<D1, D2>(State s, Cons<D1, D2> other) where D1 : Term where D2 : Term => false;
        //public virtual bool SameAs<T>(State s, Cont<T> other) where T : Term => false;
        //public virtual bool SameAs(State s, Seq other) => false;

        //A term is concrete if it has a definite value regardless of contextual state.
        //Variables are never concrete
        //A cons is concrete if it contains no variables within its entire tree.
        //Everything else is concrete.

        public virtual uint Ungroundedness { get; } = 0;
        public int CompareTo(IGrounded? other) => this.Ungroundedness.CompareTo(other?.Ungroundedness ?? 0);

        #endregion


        #region Unification
        public abstract bool TryUnifyWith(State s, Term other, out State result);

        public virtual bool TryUnifyWith(State s, Variable other, out State result) =>
            other.SameAs(s, this)
            ? s.Affirm(other, this, out result)
            : s.TryExtend(other, this, out result);

        public virtual bool TryUnifyWith<D>(State s, Value<D> other, out State result) => s.Reject(other, this, out result);
        public virtual bool TryUnifyWith(State s, Number other, out State result) => s.Reject(other, this, out result);
        public virtual bool TryUnifyWith(State s, Nil other, out State result) => s.Reject(other, this, out result);
        public virtual bool TryUnifyWith(State s, LCons other, out State result) => s.Reject(other, this, out result);
        public virtual bool TryUnifyWith<D1, D2>(State s, Cons<D1, D2> other, out State result)
            where D1 : Term
            where D2 : Term
        {
            return s.Reject(other, this, out result);
        }

        public virtual bool TryUnifyWith<T>(State s, IUnifiable<T> other, out State result) where T : Term => s.Reject(other.Identity, this, out result);

        public virtual bool TryUnifyWith<T>(State s, Variable<T> other, out State result) where T : Term =>
            other.SameAs(s, this)
            ? s.Affirm(other, this, out result)
            : s.TryExtend(other, this, out result);

        public virtual bool TryUnifyWith(State s, Variable<Variable> other, out State result) => other.TryUnifyWith(s, this, out result);
        public virtual bool TryUnifyWith<D>(State s, Variable<Value<D>> other, out State result) => other.TryUnifyWith(s, this, out result);
        public virtual bool TryUnifyWith(State s, Variable<Number> other, out State result) => other.TryUnifyWith(s, this, out result);
        public virtual bool TryUnifyWith(State s, Variable<Nil> other, out State result) => s.Reject(other, this, out result);
        public virtual bool TryUnifyWith(State s, Variable<LCons> other, out State result) => other.TryUnifyWith(s, this, out result);
        public virtual bool TryUnifyWith<D1, D2>(State s, Variable<Cons<D1, D2>> other, out State result)
            where D1 : Term
            where D2 : Term
        {
            return other.TryUnifyWith(s, this, out result);
        }

        #endregion

        #region Standard Overrides

        public abstract override string ToString();
        public virtual string ToVerboseString() => this.ToString();

        public IEnumerable<string> ToTree() => this.ToTree("", true, false);
        public virtual IEnumerable<string> ToTree(string prefix, bool first, bool last) => new string[]
        {
            last
            ? prefix + IO.LEAVES + this.ToString()
            : prefix + IO.BRANCH + this.ToString()
        };


        public override bool Equals(object? obj) => ReferenceEquals(obj, this);
        public override int GetHashCode() => this.ToString().GetHashCode();

        #endregion

        #region Implicit Conversions

        public static implicit operator Term(int i) => new Number(i);
        public static implicit operator Term(double d) => new Number(d);
        public static implicit operator Term((int, int) ii) => new Number(ii.Item1, ii.Item2);

        public static implicit operator Term(char c) => ValueFactory.Box(c);
        public static implicit operator Term(bool b) => b ? Term.True : Term.False;
        public static implicit operator Term(string s) => LCons.Truct(s);
        //public static implicit operator Term(string s) => new Seq(s.Select(x => ValueFactory.Box(x)).ToArray());

        #endregion

        #region Operator Overloads

        public static Goals.Goal operator ==(Term left, Term right) => new Goals.Equality(left, right);
        public static Goals.Goal operator !=(Term left, Term right) => new Goals.Disequality(left, right);

        #endregion
    }


    public interface IMaybe<T1, T2>
        where T1 : Term
        where T2 : Term
    {
        Term Reflect();
        void Switch(Action<T1> caseA, Action<T2> caseB);
        R Match<R>(Func<T1, R> caseA, Func<T2, R> caseB);

        bool SameAs(State s, Term other);
        State? UnifyWith(State s, Term other);
    }
}
