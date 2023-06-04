using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    public abstract class Explication<T> : Combination<T>
        where T : Goal
    {
        protected Explication(params T[] goals) : base(goals) { }
    }

    public class Implication<T> : Explication<T>
        where T : Goal
    {
        public override string Expression => $"{this.Implicand} <- {String.Join(", ", this.InnerGoal.ChildGoals)}";
        public override string Description => "The first statement is implied by the rest";

        private T? Implicand = null;
        private Disjunction<Goal> InnerGoal = new();

        public Implication(T conclusion, params T[] hypotheses) : base(hypotheses)
        {
            this.Implicand = conclusion;

            foreach(var hypo in hypotheses)
            {
                InnerGoal.Add(hypo.Negate());
            }
        }

        protected override Type NonReflectiveType => typeof(Impl);

        protected override Stream Application(State s)
        {
            if (ReferenceEquals(this.Implicand, null))
            {
                return Stream.Empty();
            }

            return Stream.Interleave(this.Implicand.PursueIn(s), this.InnerGoal.PursueIn(s));
        }

        public override Goal Negate()
        {
            return this.InnerGoal.Negate();
        }

        public override void Add(T goal)
        {
            if (ReferenceEquals(this.Implicand, null))
            {
                this.Implicand = goal;
            }

            this.InnerGoal.Add(goal.Negate());
        }
    }

    public sealed class Impl : Implication<Goal>
    {
        public Impl(Goal conclusion, params Goal[] hypotheses) : base(conclusion, hypotheses) { }
    }

    public class BiImplication<T> : Explication<T>
        where T : Goal
    {
        public override string Expression => String.Join(" <=> ", this.SubGoals);

        public override string Description => "All of the following statements are either all true or all false";

        public BiImplication(params T[] goals) : base(goals) { }

        protected override Type NonReflectiveType => typeof(BImp);

        protected override Stream Application(State s)
        {
            if (!this.SubGoals.Any())
            {
                return new Bottom().PursueIn(s);
            }
            else if (this.SubGoals.Count() == 1)
            {
                return new Top().PursueIn(s);
            }
            else
            {
                return new Disj()
                {
                    new Conj(this.SubGoals.ToArray()),
                    new Conj(this.SubGoals.Select(x => x.Negate()).ToArray())
                }.PursueIn(s);
            }
        }

        public override Goal Negate()
        {
            return new ExclusiveDisjunction<T>(this.SubGoals.ToArray());
        }

        public override void Add(T goal)
        {
            this.SubGoals.Add(goal);
        }
    }

    public sealed class BImp : BiImplication<Goal>
    {
        public BImp(params Goal[] goals) : base(goals) { }
    }

    public class ExclusiveDisjunction<T> : Explication<T>
        where T : Goal
    {

        public override string Expression => String.Join(" XOR ", this.SubGoals);

        public override string Description => "Exactly one of these statements is true";

        private Conjunction<Disj> InnerGoal = new();

        public ExclusiveDisjunction(params T[] goals)
        {
            foreach(T goal in goals)
            {
                this.Add(goal);
            }
        }

        protected override Type NonReflectiveType => typeof(XOR);

        protected override Stream Application(State s)
        {
            if (!this.SubGoals.Any())
            {
                return new Bottom().PursueIn(s);
            }
            else if (this.SubGoals.Count() == 1)
            {
                return new Top().PursueIn(s);
            }
            else
            {
                Disj useAny = new Disj(this.SubGoals.ToArray());
                return Conj.Aggregate(this.InnerGoal.PursueIn, useAny.PursueIn)(s);
            }

        }

        public override Goal Negate()
        {
            return new BiImplication<T>(this.SubGoals.ToArray());
        }

        public override void Add(T goal)
        {
            if (this.SubGoals.Any())
            {
                foreach(T option in this.SubGoals)
                {
                    this.InnerGoal.Add(new Disj(option.Negate(), goal.Negate()));
                }
            }

            this.SubGoals.Add(goal);
        }
    }

    public sealed class XOR : ExclusiveDisjunction<Goal>
    {
        public XOR(params Goal[] goals) : base(goals) { }
    }
}
