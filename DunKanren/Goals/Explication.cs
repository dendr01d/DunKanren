using System;
using System.Collections.Generic;
using System.Linq;

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
        public override string Expression => $"{this.Implicand} <- {String.Join(", ", this.SubExpressions)}";
        public override string Description => "The first statement is implied by the rest";

        private T Implicand;

        public Implication(T conclusion, params T[] hypotheses) : base(hypotheses)
        {
            this.Implicand = conclusion;
        }

        internal override Func<State, Stream> GetApp()
        {
            return (State s) => Disjunction<T>.Aggregate(this.Implicand.GetApp(), this.Subs.Select(x => x.Value.GetNeg()).Aggregate(Disjunction<T>.Aggregate))(s);
        }

        internal override Func<State, Stream> GetNeg()
        {
            return (State s) => Conjunction<T>.Aggregate(this.Implicand.GetNeg(), this.Subs.Select(x => x.Value.GetApp()).Aggregate(Conjunction<T>.Aggregate))(s);
        }
    }

    public sealed class Impl : Implication<Goal>
    {
        public Impl(Goal conclusion, params Goal[] hypotheses) : base(conclusion, hypotheses) { }
    }

    public class BiImplication<T> : Explication<T>
        where T : Goal
    {
        public override string Expression => String.Join(" <=> ", this.SubExpressions);
        public override string Description => "All of the following statements are either all true or all false";

        public BiImplication(params T[] goals) : base(goals) { }

        internal override Func<State, Stream> GetApp()
        {
            return (State s) => Disjunction<T>.Aggregate(
                    this.Subs.Select(x => x.Value.GetApp()).Aggregate(Conjunction<T>.Aggregate),
                    this.Subs.Select(x => x.Value.GetNeg()).Aggregate(Conjunction<T>.Aggregate))(s);
        }

        internal override Func<State, Stream> GetNeg()
        {
            return (State s) => Conjunction<T>.Aggregate(
                    this.Subs.Select(x => x.Value.GetNeg()).Aggregate(Disjunction<T>.Aggregate),
                    this.Subs.Select(x => x.Value.GetApp()).Aggregate(Disjunction<T>.Aggregate))(s);
        }
    }

    public sealed class BImp : BiImplication<Goal>
    {
        public BImp(params Goal[] goals) : base(goals) { }
    }

    public class ExclusiveDisjunction<T> : Explication<T>
        where T : Goal
    {
        public override string Expression => String.Join(" XOR ", this.SubExpressions);
        public override string Description => "Exactly one of these statements is true";

        public ExclusiveDisjunction(params T[] goals) : base(goals) { }

        internal override Func<State, Stream> GetApp()
        {
            List<Func<State, Stream>> subApps = new();

            //for every pair of subgoals, assert that one of them must be false
            for (int i = 0; i < this.Subs.Count() - 1; ++i)
            {
                for (int j = i + 1; j < this.Subs.Count(); ++i)
                {
                    subApps.Add(Disjunction<T>.Aggregate(this.Subs[i].Value.GetNeg(), this.Subs[j].Value.GetNeg()));
                }
            }

            //assert that at least one of the subgoals is true
            subApps.Add(this.Subs.Select(x => x.Value.GetApp()).Aggregate(Disjunction<T>.Aggregate));

            //conjoin all of these assertions
            return subApps.Aggregate(Conjunction<T>.Aggregate);
        }

        internal override Func<State, Stream> GetNeg()
        {
            List<Func<State, Stream>> subApps = new();

            //for every pair of subgoals, assert that both of them must be true
            for (int i = 0; i < this.Subs.Count() - 1; ++i)
            {
                for (int j = i + 1; j < this.Subs.Count(); ++i)
                {
                    subApps.Add(Conjunction<T>.Aggregate(this.Subs[i].Value.GetApp(), this.Subs[j].Value.GetApp()));
                }
            }

            //assert that any one of these dual-assertions must be true
            return subApps.Aggregate(Disjunction<T>.Aggregate);
        }
    }

    public sealed class XOR : ExclusiveDisjunction<Goal>
    {
        public XOR(params Goal[] goals) : base(goals) { }
    }
}
