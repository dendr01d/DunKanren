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
    public abstract class Term : IPrintable
    {
        public static readonly Value<bool> True = new(true);
        public static readonly Value<bool> False = new(false);

        public static readonly Nil nil = new();

        public virtual Term Dereference(State s) => this;


        #region Equivalence
        public abstract bool SameAs(State s, Term other);

        public virtual bool SameAs(State s, Variable other) => false;
        public virtual bool SameAs<D>(State s, Variable<D> other) => false;
        public virtual bool SameAs<D>(State s, Value<D> other) => false;
        public virtual bool SameAs(State s, Nil other) => false;
        public virtual bool SameAs(State s, Cons other) => false;
        //public virtual bool SameAs<T>(State s, Cont<T> other) where T : Term => false;

        #endregion


        #region Unification
        public abstract State? UnifyWith(State s, Term other);

        public virtual State? UnifyWith(State s, Variable other) => other.SameAs(s, this) ? s.Affirm(other, this) : s.Extend(other, this);
        public virtual State? UnifyWith<D>(State s, Variable<D> other) => s.Reject(other, this);
        public virtual State? UnifyWith<D>(State s, Value<D> other) => s.Reject(other, this);
        public virtual State? UnifyWith(State s, Nil other) => s.Reject(other, this);
        public virtual State? UnifyWith(State s, Cons other) => s.Reject(other, this);
        //public virtual State? UnifyWith<T>(State s, Cont<T> other) where T : Term => s.Reject(other, this);

        #endregion

        #region Standard Overrides

        public abstract override string ToString();
        public virtual string ToVerboseString() => this.ToString();

        public IEnumerable<string> ToTree() => this.ToTree("", true, false);
        public virtual IEnumerable<string> ToTree(string prefix, bool first, bool last) => new string[] { prefix + this.ToString() };


        public override bool Equals(object? obj) => ReferenceEquals(obj, this);
        public override int GetHashCode() => this.ToString().GetHashCode();

        #endregion

        #region Implicit Conversions

        public static implicit operator Term(int i) => ValueFactory.Box(i);
        public static implicit operator Term(char c) => ValueFactory.Box(c);
        public static implicit operator Term(bool b) => b ? Term.True : Term.False;
        //public static implicit operator Term(string s) => Cons.Truct(s);
        public static implicit operator Term(string s) => Cons.Truct(s);

        #endregion

        #region Operator Overloads

        public static Goal operator ==(Term left, Term right) => Goal.Equality(left, right);
        public static Goal operator !=(Term left, Term right) => Goal.Disequality(left, right);

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
