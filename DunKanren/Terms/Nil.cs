using System;

namespace DunKanren
{
    public class Nil : Term
    {
        public override bool TermEquals(State s, Term other) => other.TermEquals(s, this);
        public override bool TermEquals(State s, Nil other) => true;

        public override string ToString() => "ⁿ";
        public override string ToVerboseString() => "nil";
    }

    public class MaybeNil<T> : Term
        where T : Term
    {
        public readonly bool IsNil;
        public readonly T? RealValue;

        public MaybeNil()
        {
            this.IsNil = true;
            this.RealValue = null;
        }

        public MaybeNil(T value)
        {
            this.IsNil = false;
            this.RealValue = value;
        }

        public R Deconstruct<R>(Func<T, R> realCase, Func<Nil, R> nilCase)
        {
            if (!this.IsNil && !ReferenceEquals(null, this.RealValue))
            {
                return realCase(this.RealValue);
            }
            return nilCase(Term.NIL);
        }

        public override uint Ungroundedness => this.Deconstruct(r => r.Ungroundedness, n => n.Ungroundedness);

        public override bool TermEquals(State s, Term other)
        {
            return this.Deconstruct(r => r.TermEquals(s, other), n => n.TermEquals(s, other));
        }

        public override string ToString()
        {
            return this.Deconstruct(r => r.ToString(), n => n.ToString());
        }
    }
}
