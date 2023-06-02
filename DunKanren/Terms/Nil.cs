using System;

namespace DunKanren
{
    public class Nil : Term
    {
        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs(State s, Nil other) => true;

        public override bool TryUnifyWith(State s, Term other, out State result) => other.TryUnifyWith(s, this, out result);
        public override bool TryUnifyWith(State s, Nil other, out State result) => s.Affirm(other, this, out result);

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

        public override bool SameAs(State s, Term other)
        {
            return this.Deconstruct(r => r.SameAs(s, other), n => n.SameAs(s, other));
        }

        public override bool TryUnifyWith(State s, Term other, out State result)
        {
            if (!this.IsNil && !ReferenceEquals(null, this.RealValue))
            {
                return this.RealValue.TryUnifyWith(s, other, out result);
            }
            return Term.NIL.TryUnifyWith(s, other, out result);
        }

        public override string ToString()
        {
            return this.Deconstruct(r => r.ToString(), n => n.ToString());
        }
    }
}
