using System;

namespace DunKanren
{
    public class Nil : Term
    {
        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs(State s, Nil other) => true;

        public override State? UnifyWith(State s, Term other) => other.UnifyWith(s, this);
        public override State? UnifyWith(State s, Nil other) => s.Affirm(other, this);

        public override string ToString() => "ⁿ";

        public override string ToVerboseString() => "nil";
    }
}
