using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    public abstract class Variation : Goal, IEnumerable<Goal>
    {
        public override IEnumerable<IPrintable> Components => this.Goals;

        protected List<Goal> Goals;
        protected Goal GetComposite() => this.Goals.Aggregate(this.Aggregator);
        private readonly Func<Goal, Goal, Goal> Aggregator;

        public Variation(Func<Goal, Goal, Goal> agg)
        {
            this.Goals = new();
            this.Aggregator = agg;
        }

        public void Add(Goal g)
        {
            this.Goals.Add(g);
        }

        public override Goal Negate() => this.GetComposite().Negate();
        protected override Stream Pursuit(State s) => this.GetComposite().PursueIn(s);

        public IEnumerator<Goal> GetEnumerator() => new List<Goal>() { this.GetComposite() }.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    }

    /// <summary>
    /// Variadic Implication, otherwise known as a Horn clause (a or !b or !c or...)
    /// </summary>
    public sealed class Horn : Variation
    {
        public override string Expression => this.Goals.Count() switch
        {
            <= 0 => "NULL",
            1 => this.Goals.First().Expression,
            >= 2 => $"{this.Goals.First()} <= {String.Join(", ", this.Goals.Select(x => x.Expression).Skip(1))}"
        };

        public override string Description => "The first statement is implied by the rest";

        public Horn(Goal positive) : base((x, y) => new Disjunction(x, y.Negate()))
        {
            this.Goals.Add(positive);
        }
    }

    /// <summary>
    /// Dual form of a Horn clause (!a or b or c or...)
    /// </summary>
    public sealed class DualHorn : Variation
    {
        public override string Expression => throw new NotImplementedException();
        public override string Description => throw new NotImplementedException();

        public DualHorn(Goal negative) : base((x, y) => new Disjunction(x, y))
        {
            this.Goals.Add(negative);
        }
    }


    public sealed class Conjoint : Variation
    {
        public override string Expression => String.Join(" && ", this.Goals);
        public override string Description => "All of the following must be true";

        public Conjoint() : base((x, y) => new Conjunction(x, y)) { }
    }

    public sealed class Disjoint : Variation
    {
        public override string Expression => String.Join(" || ", this.Goals);
        public override string Description => "At least one of the following must be true";
        public Disjoint() : base((x, y) => new Disjunction(x, y)) { }
    }


    /// <summary>
    /// Variadic Conjunction with the capability to accept subranges as disjunctions. AKA
    /// Conjunctive Normal Form
    /// </summary>
    public sealed class NormalConjunctive : Variation
    {
        public override string Expression => String.Join(" && ", this.Goals);
        public override string Description => "All of the following must be true";
        public NormalConjunctive() : base((x, y) => new Conjunction(x, y)) { }
        public void Add(params Goal[] goals)
        {
            this.Goals.Add(goals.Aggregate((x, y) => new Disjunction(x, y)));
        }
    }

    /// <summary>
    /// Variadic disjunction with the capability to accept subranges as conjunctions. AKA
    /// Disjunctive Normal Form
    /// </summary>
    public sealed class NormalDisjunctive : Variation
    {
        public override string Expression => String.Join(" || ", this.Goals);
        public override string Description => "At least one of the following must be true";
        public NormalDisjunctive() : base((x, y) => new Disjunction(x, y)) { }
        public void Add(params Goal[] goals)
        {
            this.Goals.Add(goals.Aggregate((x, y) => new Conjunction(x, y)));
        }
    }
}
