using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    internal abstract class Relation : Goal<Term>
    {
        public override IEnumerable<IPrintable> ChildGoals => this.GetList();

        protected Term LeftArg;
        protected Term RightArg;

        public override Stream PursueIn(State s)
        {
            IO.Debug_Print(this.ToString());

            return base.PursueIn(s);
        }

        protected Relation(Term arg1, Term arg2) : base()
        {
            this.LeftArg = arg1;
            this.RightArg = arg2;
        }

        public override IEnumerator<Term> GetEnumerator() => this.GetList().GetEnumerator();

        private List<Term> GetList() => new List<Term>() { this.LeftArg, this.RightArg };

        public override int Ungroundedness => LeftArg.Ungroundedness + RightArg.Ungroundedness;
    }

    /// <summary>
    /// Represents the unification of two terms
    /// </summary>
    internal class Equality : Relation
    {
        public override string Expression => $"{this.LeftArg} ≡ {this.RightArg}";
        public override string Description => "The following terms are equivalent";

        public Equality(Term lhs, Term rhs) : base(lhs, rhs) { }

        public override Goal<Term> Negate()
        {
            return new Disequality(this.LeftArg, this.RightArg);
        }

        protected override Stream Application(State s)
        {
            if (s.TryUnify(this.LeftArg, this.RightArg, out State result))
            {
                return Stream.Singleton(result);
            }

            return Stream.Empty();
        }

        protected override Type NonReflectiveType => typeof(Equality);
    }

    /// <summary>
    /// Represents the dis-unification of two terms
    /// </summary>
    internal class Disequality : Relation
    {
        public override string Expression => String.Join(" != ", this.ChildGoals);
        public override string Description => "The following terms are NOT equivalent";

        public Disequality(Term lhs, Term rhs) : base(lhs, rhs) { }

        public override Goal Negate()
        {
            return new Equality(this.LeftArg, this.RightArg);
        }

        protected override Stream Application(State s)
        {
            if (s.TryDisUnify(this.LeftArg, this.RightArg, out State result))
            {
                return Stream.Singleton(result);
            }

            return Stream.Empty();
        }

        protected override Type NonReflectiveType => typeof(Disequality);
    }
}
