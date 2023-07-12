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
    public abstract class Term : IPrintable, IGrounded, IEquatable<Term>
    {
        public static readonly Value<bool> True = new(true);
        public static readonly Value<bool> False = new(false);

        public static readonly Nil NIL = new();

        public Term Identity => this;

        #region Equivalence
        public abstract bool Equals(Term? other);
        public virtual Term Reify(State s) => this;

        public virtual uint Ungroundedness { get; } = 0;
        public int CompareTo(IGrounded? other) => this.Ungroundedness.CompareTo(other?.Ungroundedness ?? 0);

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


        #endregion

        #region Implicit Conversions

        public static implicit operator Term(int i) => i >= 0 ? Cons.Truct((uint)i) : ValueFactory.Box(i);
        public static implicit operator Term(double d) => new Number(d);
        public static implicit operator Term((int, int) ii) => new Number(ii.Item1, ii.Item2);

        public static implicit operator Term(char c) => ValueFactory.Box(c);
        public static implicit operator Term(bool b) => b ? True : False;
        public static implicit operator Term(string s) => Cons.Truct(s);
        //public static implicit operator Term(string s) => new Seq(s.Select(x => ValueFactory.Box(x)).ToArray());

        #endregion

        #region Operator Overloads

        public static Goals.Goal operator ==(Term left, Term right) => new Goals.Equality(left, right);
        public static Goals.Goal operator !=(Term left, Term right) => new Goals.Disequality(left, right);


        public override bool Equals(object? o) => ReferenceEquals(this, o);
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
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
