using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    internal abstract class Relation : Goal<Term>
    {
        protected Term LeftArg;
        protected Term RightArg;

        public override Stream PursueIn(State s)
        {
            return base.PursueIn(s);
        }

        protected Relation(Term arg1, Term arg2) : base()
        {
            this.LeftArg = arg1;
            this.RightArg = arg2;
        }

        public override IEnumerator<Term> GetEnumerator() => this.GetList().GetEnumerator();

        private List<Term> GetList() => new List<Term>() { this.LeftArg, this.RightArg };

        public override int Ungroundedness => int.Min(LeftArg.Ungroundedness, RightArg.Ungroundedness);
    }

    /// <summary>
    /// Represents the unification of two terms
    /// </summary>
    internal class Equality : Relation
    {
        public override string Expression => $"{this.LeftArg} ≡ {this.RightArg}";
        public override string Description => "The following terms are equivalent";

        public Equality(Term lhs, Term rhs) : base(lhs, rhs) { }

        internal override Func<State, Stream> GetApp() => (State s) => Equality.Assert(s, this.LeftArg, this.RightArg);
        internal override Func<State, Stream> GetNeg() => (State s) => Disequality.Assert(s, this.LeftArg, this.RightArg);

        public static Stream Assert(State s, Term left, Term right)
        {
            if (s.TryUnify(left, right, out State result))
            {
                return Stream.Singleton(result);
            }
            return Stream.Empty();
        }
    }

    /// <summary>
    /// Represents the dis-unification of two terms
    /// </summary>
    internal class Disequality : Relation
    {
        public override string Expression => String.Join(" != ", this.SubExpressions);
        public override string Description => "The following terms are NOT equivalent";

        public Disequality(Term lhs, Term rhs) : base(lhs, rhs) { }

        internal override Func<State, Stream> GetApp() => (State s) => Disequality.Assert(s, this.LeftArg, this.RightArg);
        internal override Func<State, Stream> GetNeg() => (State s) => Equality.Assert(s, this.LeftArg, this.RightArg);

        public static Stream Assert(State s, Term left, Term right)
        {
            if (s.TryDisUnify(left, right, out State result))
            {
                return Stream.Singleton(result);
            }
            return Stream.Empty();
        }
    }
}
