using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    /// <summary>
    /// An simple goal is one that performs operations on atomic goals and other simple goals.
    /// Like atomic goals, they are negatable.
    /// </summary>
    public abstract class SimpleGoal : ComplexGoal
    {
        protected SimpleGoal() : base() { }
        protected SimpleGoal(Func<State, Stream> app) : base(app) { }

        public abstract SimpleGoal Negate();

    }

    public class Negation : SimpleGoal
    {
        private readonly SimpleGoal Operand;

        public Negation(SimpleGoal g) : base(s => g.Negate().PursueIn(s))
        {
            this.Operand = g;
            this.Expression = "¬ " + Operand.ToString();
            this.Description = "The following statement is FALSE:";
            this.Children = new List<IPrintable>() { this.Operand };
        }

        public override SimpleGoal Negate() => this.Operand;
    }

    public abstract class BinarySimpleGoal : SimpleGoal
    {
        protected readonly SimpleGoal LeftOperand;
        protected readonly SimpleGoal RightOperand;

        protected BinarySimpleGoal(SimpleGoal left, SimpleGoal right, Func<SimpleGoal, SimpleGoal, Func<State, Stream>> combiner, string op, string desc) :
            base(combiner(left, right))
        {
            this.LeftOperand = left;
            this.RightOperand = right;
            this.Expression = "(" + LeftOperand.ToString() + " " + op +  " " + RightOperand.ToString() + ")";
            this.Description = desc;
            this.Children = new List<IPrintable>() { left, right };
        }

    }

    #region binary logical operators

    public class Conjunction : BinarySimpleGoal
    {
        private static Func<State, Stream> Conj(SimpleGoal left, SimpleGoal right)
        {
            return (State s) => Stream.New(left.PursueIn(s).SelectMany(x => right.PursueIn(x)));
        }

        public Conjunction(SimpleGoal left, SimpleGoal right) :
            base(left, right, Conj, @"/\", "Both of these statements are true:")
        { }

        //NOT (A and B) --> (NOT A) or (NOT B)
        public override SimpleGoal Negate() => new Disjunction(LeftOperand.Negate(), RightOperand.Negate());
    }

    public class Disjunction : BinarySimpleGoal
    {
        private static Func<State, Stream> Disj(SimpleGoal left, SimpleGoal right)
        {
            return (State s) => Stream.Interleave(left.PursueIn(s), right.PursueIn(s));
        }

        public Disjunction(SimpleGoal left, SimpleGoal right) :
            base(left, right, Disj, @"\/", "One or both of these statements are true:")
        { }

        //NOT (A or B) --> (NOT A) and (NOT B)
        public override SimpleGoal Negate() => new Conjunction(LeftOperand.Negate(), RightOperand.Negate());
    }

    public class Implication : BinarySimpleGoal
    {
        private static Func<State, Stream> Impl(SimpleGoal left, SimpleGoal right)
        {
            return (State s) => new Disjunction(left.Negate(), right).PursueIn(s);
        }

        public Implication(SimpleGoal antecedent, SimpleGoal consequent) :
            base(antecedent, consequent, Impl, "══>", "The first of the following statements implies the second:")
        { }
        public override SimpleGoal Negate() => new NonImplication(LeftOperand, RightOperand);
    }

    public class NonImplication : BinarySimpleGoal
    {
        private static Func<State, Stream> NImpl(SimpleGoal left, SimpleGoal right)
        {
            return (State s) => new Conjunction(left, right.Negate()).PursueIn(s);
        }

        public NonImplication(SimpleGoal antecedent, SimpleGoal consequent) :
            base(antecedent, consequent, NImpl, "═╪>", "The first of the following statements does NOT imply the second:")
        { }
        public override SimpleGoal Negate() => new Implication(LeftOperand, RightOperand);
    }

    public class BiImplication : BinarySimpleGoal
    {
        private static Func<State, Stream> BImpl(SimpleGoal left, SimpleGoal right)
        {
            return (State s) => new Disjunction(new Conjunction(left, right), new Conjunction(left.Negate(), right.Negate())).PursueIn(s);
        }

        public BiImplication(SimpleGoal antecedent, SimpleGoal consequent) :
            base(antecedent, consequent, BImpl, "<═>", "The first of the following statements is true if and only if the second is true:")
        { }
        public override SimpleGoal Negate() => new ExclusiveDisjunction(LeftOperand.Negate(), RightOperand.Negate());
    }

    public class ExclusiveDisjunction : BinarySimpleGoal
    {
        private static Func<State, Stream> XOR(SimpleGoal left, SimpleGoal right)
        {
            return (State s) => new Conjunction(new Disjunction(left, right), new Disjunction(left.Negate(), right.Negate())).PursueIn(s);
        }

        public ExclusiveDisjunction(SimpleGoal antecedent, SimpleGoal consequent) :
            base(antecedent, consequent, XOR, "(┼)", "One of the following statements is true, but NOT both:")
        { }
        public override SimpleGoal Negate() => new BiImplication(LeftOperand.Negate(), RightOperand.Negate());
    }

    #endregion


    public abstract class VariadicSimpleGoal : SimpleGoal, IEnumerable<SimpleGoal>
    {
        private readonly string Operator;
        private string PrintChildren()
        {
            return "(" + String.Join(" " + this.Operator + " ", this.Children.Select(x => x.ToString())) + ")";
        }

        protected List<SimpleGoal> SubGoals;
        protected readonly BinarySimpleGoal Aggregator;
        protected Stream AggregateSubGoals(State s)
        {
            return this.SubGoals.Aggregate((x, y) => this.Aggregator.)
        }

        protected VariadicSimpleGoal(IEnumerable<SimpleGoal> goals, BinarySimpleGoal agg, string op, string desc)
        {
            this.SubGoals = new List<SimpleGoal>(goals);
            this.Aggregator = agg;
            this.Application = AggregateSubGoals;

            this.Children = new List<IPrintable>(goals);

            this.Operator = op;
            this.Expression = this.PrintChildren();
            this.Description = desc;
        }
        public virtual void Add(SimpleGoal g)
        {
            this.Children.Add(g);
            this.SubGoals.Add(g);
        }

        public IEnumerator<SimpleGoal> GetEnumerator() => this.SubGoals.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.SubGoals.GetEnumerator();
    }
}
