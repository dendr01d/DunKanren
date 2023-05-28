using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    internal abstract class Relation : Goal<Term, Term>
    {
        public override IEnumerable<IPrintable> Components =>
            new List<IPrintable>() { this.Argument1, this.Argument2 };

        protected Relation(Term arg1, Term arg2) : base(arg1, arg2) { }
    }

    /// <summary>
    /// Represents the unification of two terms
    /// </summary>
    internal class Equality : Relation
    {
        public override string Expression => String.Join(" == ", this.Components);
        public override string Description => "The following terms are equivalent";

        public Equality(Term arg1, Term arg2) : base(arg1, arg2) { }

        public override Goal<Term, Term> Negate()
        {
            return new Disequality(this.Argument1, this.Argument2);
        }

        protected override Stream ApplyToState(State s, Term arg1, Term arg2)
        {
            if (s.TryUnify(arg1, arg2, out State output))
            {
                return Stream.Singleton(output);
            }
            else
            {
                return Stream.Empty();
            }
        }
    }

    /// <summary>
    /// Represents the dis-unification of two terms
    /// </summary>
    internal class Disequality : Relation
    {
        public override string Expression => String.Join(" != ", this.Components);
        public override string Description => "The following terms are NOT equivalent";

        public Disequality(Term arg1, Term arg2) : base(arg1, arg2) { }

        public override Goal<Term, Term> Negate()
        {
            return new Equality(this.Argument1, this.Argument2);
        }

        protected override Stream ApplyToState(State s, Term arg1, Term arg2)
        {
            if (s.TryDisUnify(arg1, arg2, out State output))
            {
                return Stream.Singleton(output);
            }
            else
            {
                return Stream.Empty();
            }
        }
    }}
