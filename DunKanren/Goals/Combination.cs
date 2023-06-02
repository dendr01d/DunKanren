using System;
using System.Collections.Generic;
using System.Linq;

namespace DunKanren.Goals
{
    public abstract class Combination<T> : Goal<T>
        where T : Goal
    {
        protected Combination(params T[] goals)
        {
            this.Subs = goals.ToList();
        }

        public override Stream PursueIn(State s)
        {
            if (!this.Subs.Any())
            {
                return Stream.Empty();
            }

            return base.PursueIn(s);
        }

        protected Lazy<T> Lazify(T g) => new Lazy<T>(() => g);

        public void Add(T item) => this.Subs.Add(item);

        public override uint Ungroundedness => (uint)this.Subs.Sum(x => x.Ungroundedness);
    }

    /// <summary>
    /// Logical Conjunction (a and b and c and...)
    /// </summary>
    public class Conjunction<T> : Combination<T>
        where T : Goal
    {
        public override string Expression => String.Join(" && ", this.SubExpressions);
        public override string Description => "All of the following are true";

        public Conjunction(params T[] goals) : base(goals) { }

        internal override Lazy<Func<State, Stream>> GetApp()
        {
            return new (() => (State s) => this.Subs.Select(x => x.GetApp().Value).Aggregate(Conjunction<T>.Aggregate)(s));
        }

        internal override Lazy<Func<State, Stream>> GetNeg()
        {
            return new (() => (State s) => this.Subs.Select(x => x.GetNeg().Value).Aggregate(Disjunction<T>.Aggregate)(s));
        }

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
        public override string Expression => String.Join(" || ", this.SubExpressions);
        public override string Description => "At least one of the following is true";

        public Disjunction(params T[] goals) : base(goals) { }

        internal override Lazy<Func<State, Stream>> GetApp()
        {
            return new(() => (State s) => this.Subs.Select(x => x.GetApp().Value).Aggregate(Disjunction<T>.Aggregate)(s));
        }

        internal override Lazy<Func<State, Stream>> GetNeg()
        {
            return new(() => (State s) => this.Subs.Select(x => x.GetNeg().Value).Aggregate(Conjunction<T>.Aggregate)(s));
        }

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
