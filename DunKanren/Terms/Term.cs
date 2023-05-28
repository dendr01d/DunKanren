﻿using DunKanren.Terms;
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
        //public virtual bool SameAs(State s, Seq other) => false;

        //A term is concrete if it has a definite value regardless of contextual state.
        //Variables are never concrete
        //A cons is concrete if it contains no variables within its entire tree.
        //Everything else is concrete.
        public abstract bool IsConcrete();

        /// <summary>
        /// If this term and the other term share concreteness (or lack thereof),
        /// returns whether or not they are equivalent. Otherwise, returns null.
        /// </summary>
        public bool? InvariantlySame(Term other)
        {
            if (this.IsConcrete() == other.IsConcrete())
            {
                return this.SameAs(State.InitialState(), other);
            }

            return null;
        }

        #endregion


        #region Unification
        public abstract bool TryUnifyWith(State s, Term other, out State result);

        public virtual bool TryUnifyWith(State s, Variable other, out State result) =>
            other.SameAs(s, this)
            ? s.Affirm(other, this, out result)
            : s.TryExtend(other, this, out result);
        public virtual bool TryUnifyWith<D>(State s, Variable<D> other, out State result) => s.Reject(other, this, out result);
        public virtual bool TryUnifyWith<D>(State s, Value<D> other, out State result) => s.Reject(other, this, out result);
        public virtual bool TryUnifyWith(State s, Nil other, out State result) => s.Reject(other, this, out result);
        public virtual bool TryUnifyWith(State s, Cons other, out State result) => s.Reject(other, this, out result);
        //public virtual State? UnifyWith<T>(State s, Cont<T> other) where T : Term => s.Reject(other, this);
        //public virtual bool TryUnifyWith(State s, Seq other, out State result) => s.Reject(other, this, out result);

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

        public static implicit operator Term(int i) => ValueFactory.Box(i);
        public static implicit operator Term(char c) => ValueFactory.Box(c);
        public static implicit operator Term(bool b) => b ? Term.True : Term.False;
        public static implicit operator Term(string s) => Cons.Truct(s);
        public static implicit operator Term(char[] cs) => Cons.Literal(cs);
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
