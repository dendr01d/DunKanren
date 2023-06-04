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

        internal override Lazy<Func<State, Stream>> GetApp()
        {
            return new(() => (State s) => Disjunction<T>
            .Aggregate(this.Implicand.GetNeg().Value, this.Subs.Select(x => x.GetApp().Value)
            .Aggregate(Disjunction<T>.Aggregate))(s));
        }

        internal override Lazy<Func<State, Stream>> GetNeg()
        {
            return new(() => (State s) => Conjunction<T>
            .Aggregate(this.Implicand.GetApp().Value, this.Subs.Select(x => x.GetNeg().Value)
                .Aggregate(Conjunction<T>.Aggregate))(s));
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

        internal override Lazy<Func<State, Stream>> GetApp()
        {
            return new(() => (State s) => Disjunction<T>.Aggregate(
                    this.Subs.Select(x => x.GetApp().Value).Aggregate(Conjunction<T>.Aggregate),
                    this.Subs.Select(x => x.GetNeg().Value).Aggregate(Conjunction<T>.Aggregate))(s));
        }

        internal override Lazy<Func<State, Stream>> GetNeg()
        {
            return new(() => (State s) => Conjunction<T>.Aggregate(
                    this.Subs.Select(x => x.GetNeg().Value).Aggregate(Disjunction<T>.Aggregate),
                    this.Subs.Select(x => x.GetApp().Value).Aggregate(Disjunction<T>.Aggregate))(s));
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

        internal override Lazy<Func<State, Stream>> GetApp()
        {
            List<Func<State, Stream>> subApps = new();

            //for every pair of subgoals, assert that one of them must be false
            for (int i = 0; i < this.Subs.Count() - 1; ++i)
            {
                for (int j = i + 1; j < this.Subs.Count(); ++j)
                {
                    subApps.Add(Disjunction<T>.Aggregate(this.Subs[i].GetNeg().Value, this.Subs[j].GetNeg().Value));
                }
            }

            //assert that at least one of the subgoals is true
            subApps.Add(this.Subs.Select(x => x.GetApp().Value).Aggregate(Disjunction<T>.Aggregate));

            //conjoin all of these assertions
            return new(() => subApps.Aggregate(Conjunction<T>.Aggregate));
        }

        internal override Lazy<Func<State, Stream>> GetNeg()
        {
            List<Func<State, Stream>> subApps = new();

            //for every pair of subgoals, assert that both of them must be true
            for (int i = 0; i < this.Subs.Count() - 1; ++i)
            {
                for (int j = i + 1; j < this.Subs.Count(); ++j)
                {
                    subApps.Add(Conjunction<T>.Aggregate(this.Subs[i].GetApp().Value, this.Subs[j].GetApp().Value));
                }
            }

            //assert that any one of these dual-assertions must be true
            return new(() => subApps.Aggregate(Disjunction<T>.Aggregate));
        }
    }

    public sealed class XOR : ExclusiveDisjunction<Goal>
    {
        public XOR(params Goal[] goals) : base(goals) { }
    }
}
