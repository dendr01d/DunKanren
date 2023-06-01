using System;
using System.Collections.Generic;
using System.Linq;

namespace DunKanren.Goals
{
    public abstract class Combination<T> : Goal<T>, ICollectionInitialized<T>
        where T : Goal
    {
        protected virtual List<T> SubGoals { get; private set; } = new();

        public override IEnumerable<IPrintable> ChildGoals => this.SubGoals;

        protected Combination(params T[] goals)
        {
            this.SubGoals = goals.ToList();
        }

        public override Stream PursueIn(State s)
        {
            if (!this.SubGoals.Any())
            {
                return Stream.Empty();
            }

            return base.PursueIn(s);
        }

        public override IEnumerator<T> GetEnumerator() => this.SubGoals.GetEnumerator();

        public abstract void Add(T goal);

        public override int Ungroundedness => this.SubGoals.Sum(x => x.Ungroundedness);
    }

    /// <summary>
    /// Logical Conjunction (a and b and c and...)
    /// </summary>
    public class Conjunction<T> : Combination<T>
        where T : Goal
    {
        public override string Expression => String.Join(" && ", this.ChildGoals);
        public override string Description => "Both of the following are true";

        public Conjunction(params T[] goals) : base(goals) { }

        public override Goal Negate()
        {
            return new Disjunction<Goal>(this.SubGoals.Select(x => x.Negate()).ToArray());
        }

        protected override Stream Application(State s)
        {
            return Stream.New(
                this.SubGoals
                .Order()
                .Aggregate(Stream.Singleton(s),
                    (xStr, xG) => Stream.New(xStr.SelectMany(xS => xG.PursueIn(xS)))));
        }

        public override void Add(T goal) => this.SubGoals.Add(goal);

        public static Func<State, Stream> Aggregate(Func<State, Stream> g1, Func<State, Stream> g2)
        {
            return (State s) => Stream.New(g1(s).SelectMany(g2));
        }
    }

    public sealed class Conj : Conjunction<Goal>
    {
        public Conj(params Goal[] goals) : base(goals) { }
    }

    public class ConjunctiveNormal<T> : Conjunction<Disjunction<T>>
        where T : Goal
    {
        public ConjunctiveNormal(params T[] goals) : base(new Disjunction<T>(goals)) { }

        public void Add(params T[] goals)
        {
            this.Add(new Disjunction<T>(goals));
        }
    }

    public sealed class CNF : ConjunctiveNormal<Goal>
    {
        public CNF(params Goal[] goals) : base(goals) { }
    }

    /// <summary>
    /// Logical Disjunction (a or b or c or...)
    /// </summary>
    public class Disjunction<T> : Combination<T>
        where T : Goal
    {
        public override string Expression => String.Join(" || ", this.ChildGoals);
        public override string Description => "At least one of the following is true";

        public Disjunction(params T[] goals) : base(goals) { }

        public override Goal Negate()
        {
            return new Conjunction<Goal>(this.SubGoals.Select(x => x.Negate()).ToArray());
        }

        protected override Stream Application(State s)
        {
            return Stream.New(this.SubGoals.Select(x => x.PursueIn(s)).Aggregate(Stream.Interleave));
        }

        public override void Add(T goal) => this.SubGoals.Add(goal);

        public static Func<State, Stream> Aggregate(Func<State, Stream> g1, Func<State, Stream> g2)
        {
            return (State s) => Stream.Interleave(g1(s), g2(s));
        }
    }

    public sealed class Disj : Disjunction<Goal>
    {
        public Disj(params Goal[] goals) : base(goals) { }
    }

    public class DisjunctiveNormal<T> : Disjunction<Conjunction<T>>
        where T : Goal
    {
        public DisjunctiveNormal(params T[] goals) : base(new Conjunction<T>(goals)) { }

        public void Add(params T[] goals)
        {
            this.Add(new Conjunction<T>(goals));
        }
    }

    public sealed class DNF : DisjunctiveNormal<Goal>
    {
        public DNF(params Goal[] goals) : base(goals) { }
    }

}
