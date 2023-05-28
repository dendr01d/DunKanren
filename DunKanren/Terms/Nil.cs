using System;

namespace DunKanren
{
    public class Nil : Term
    {
        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs(State s, Nil other) => true;

        public override bool TryUnifyWith(State s, Term other, out State result) => other.TryUnifyWith(s, this, out result);
        public override bool TryUnifyWith(State s, Nil other, out State result) => s.Affirm(other, this, out result);

        public override bool IsConcrete() => true;

        public override string ToString() => "ⁿ";
        public override string ToVerboseString() => "nil";
    }
}
