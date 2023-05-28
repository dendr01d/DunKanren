using System;
using System.Collections.Generic;
using System.Linq;

namespace DunKanren.Goals
{
    public abstract class Combination : Goal<Goal, Goal>
    {
        public override IEnumerable<IPrintable> Components =>
            new List<IPrintable>() { this.Argument1, this.Argument2 };

        protected Combination(Goal arg1, Goal arg2) : base(arg1.Resolve(), arg2.Resolve()) { }
    }

    /// <summary>
    /// Logical Conjunction (a and b and c and...)
    /// </summary>
    public sealed class Conjunction : Combination
    {
        public override string Expression => String.Join(" && ", this.Components);
        public override string Description => "Both of the following are true";

        public Conjunction(Goal arg1, Goal arg2) : base(arg1, arg2) { }

        public override Goal<Goal, Goal> Negate()
        {
            return new Disjunction(this.Argument1.Negate(), this.Argument2.Negate());
        }

        protected override Stream ApplyToState(State s, Goal arg1, Goal arg2)
        {
            return Stream.New(arg1.PursueIn(s).SelectMany(arg2.PursueIn));
        }
    }

    /// <summary>
    /// Logical Disjunction (a or b or c or...)
    /// </summary>
    public sealed class Disjunction : Combination
    {
        public override string Expression => String.Join(" || ", this.Components);
        public override string Description => "At least one of the following is true";

        public Disjunction(Goal arg1, Goal arg2) : base(arg1, arg2) { }

        public override Goal<Goal, Goal> Negate()
        {
            return new Conjunction(this.Argument1.Negate(), this.Argument2.Negate());
        }

        protected override Stream ApplyToState(State s, Goal arg1, Goal arg2)
        {
            return Stream.Interleave(arg1.PursueIn(s), arg2.PursueIn(s));
        }
    }

    public sealed class Implication : Combination
    {
        public override string Expression => $"{this.Argument1} => {this.Argument2}";
        public override string Description => "The first statement implies the second";

        public Implication(Goal arg1, Goal arg2) : base(arg1, arg2) { }

        protected override Stream ApplyToState(State s, Goal arg1, Goal arg2)
        {
            return new Disjunction(this.Argument1.Negate(), this.Argument2).PursueIn(s);
        }
        public override Goal Negate()
        {
            return new Falsification(this.Argument1, this.Argument2);
        }
    }

    public sealed class Falsification : Combination
    {
        public override string Expression => $"{this.Argument1} , {this.Argument2}";
        public override string Description => "There is no inherent relationship between the following";
        public Falsification(Goal arg1, Goal arg2) : base(arg1, arg2) { }

        protected override Stream ApplyToState(State s, Goal arg1, Goal arg2)
        {
            return new Conjunction(this.Argument1, this.Argument2.Negate()).PursueIn(s);
        }
        public override Goal Negate()
        {
            return new Implication(this.Argument1, this.Argument2);
        }
    }

    public sealed class BiImplication : Combination
    {
        public override string Expression => $"{this.Argument1} <=> {this.Argument2}";
        public override string Description => "The first statement is true if (and only if) the second is true";

        public BiImplication(Goal arg1, Goal arg2) : base(arg1, arg2) { }

        protected override Stream ApplyToState(State s, Goal arg1, Goal arg2)
        {
            return new Conjunction(new Implication(arg1, arg2), new Implication(arg2, arg1)).PursueIn(s);
        }

        public override Goal Negate()
        {
            return new Connective(this.Argument1, this.Argument2);
        }
    }

    public sealed class Connective : Combination
    {
        public override string Expression => $"{this.Argument1} XOR {this.Argument2}";
        public override string Description => "One of these statements is true, and the other is false";

        public Connective(Goal arg1, Goal arg2) : base(arg1, arg2) { }

        protected override Stream ApplyToState(State s, Goal arg1, Goal arg2)
        {
            return new Disjunction(
                new Falsification(this.Argument1, this.Argument2),
                new Falsification(this.Argument2, this.Argument1)).PursueIn(s);
        }

        public override Goal Negate()
        {
            return new BiImplication(this.Argument1, this.Argument2);
        }
    }
}
