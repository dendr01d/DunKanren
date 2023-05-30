using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    /// <summary>
    /// An atomic goal is one that performs a trivial operation on strictly terms.
    /// Namely top/bottom, unification, disunification, or type-checking.
    /// The truly important thing is that these goals are all trivially negatable
    /// </summary>
    public abstract class AtomicGoal : SimpleGoal
    {
        protected AtomicGoal(Func<State, Stream> app) : base(app) { }

    }

    #region arbitrary True/False goals

    public class Top : AtomicGoal
    {
        public Top() : base(s => Stream.Singleton(s)) { }
        public override AtomicGoal Negate() => new Bottom();
    }

    public class Bottom : AtomicGoal
    {
        public Bottom() : base(s => Stream.Empty()) { }
        public override AtomicGoal Negate() => new Top();
    }

    #endregion

    public abstract class UnaryAtomicGoal : AtomicGoal
    {
        protected Term Operand;

        protected UnaryAtomicGoal(Term operand, Func<State, bool> app, string op, string longOp) :
            base (s => Stream.PassThrough(s, app(s)))
        {
            this.Operand = operand;
            this.Expression = "(" + operand.ToString() + " " + op + ")";
            this.Description = "The following term is " + longOp + ":";
            this.Children = new List<IPrintable>() { operand };
        }
    }


    #region Type-checking

    public class Symbolo : UnaryAtomicGoal
    {
        public Symbolo(Term t) : base(t, s => t.IsSymbol(s), "is SYMBOL", "a Symbol object") { }
        public override AtomicGoal Negate() => new NotSymbolo(this.Operand);
    }

    public class NotSymbolo : UnaryAtomicGoal
    {
        public NotSymbolo(Term t) : base(t, s => !t.IsSymbol(s), "not SYMBOL", "not a Symbol object") { }
        public override AtomicGoal Negate() => new Symbolo(this.Operand);
    }

    public class Numbero : UnaryAtomicGoal
    {
        public Numbero(Term t) : base(t, s => !t.IsNumber(s), "is NUMBER", "a Number object") { }
        public override AtomicGoal Negate() => new NotNumbero(this.Operand);
    }

    public class NotNumbero : UnaryAtomicGoal
    {
        public NotNumbero(Term t) : base(t, s => !t.IsNumber(s), "not NUMBER", "not a Number object") { }
        public override AtomicGoal Negate() => new Numbero(this.Operand);
    }

    public class Emptyo : UnaryAtomicGoal
    {
        public Emptyo(Term t) : base(t, s => t.IsEmpty(s), "is NIL", "a Nil object") { }
        public override AtomicGoal Negate() => new NotEmptyo(this.Operand);
    }

    public class NotEmptyo : UnaryAtomicGoal
    {
        public NotEmptyo(Term t) : base(t, s => !t.IsEmpty(s), "not NIL", "not a Nil object") { }
        public override AtomicGoal Negate() => new Emptyo(this.Operand);
    }

    public class Pairo : UnaryAtomicGoal
    {
        public Pairo(Term t) : base(t, s => t.IsPair(s), "is PAIR", "a Pair object") { }
        public override AtomicGoal Negate() => new NotPairo(this.Operand);
    }

    public class NotPairo : UnaryAtomicGoal
    {
        public NotPairo(Term t) : base(t, s => t.IsPair(s), "not PAIR", "not a Pair object") { }
        public override AtomicGoal Negate() => new Pairo(this.Operand);
    }

    #endregion

    public abstract class BinaryAtomicGoal : AtomicGoal
    {
        protected Term LeftOperand;
        protected Term RightOperand;

        protected BinaryAtomicGoal(Term left, Term right, Func<State, State?> app, string op, string longOp) :
            base(s => Stream.Singleton(app(s)))
        {
            this.LeftOperand = left;
            this.RightOperand = right;
            this.Expression = "(" + left.ToString() + " " + op + " " + right.ToString() + ")";
            this.Description = "The following terms are " + longOp + ":";
            this.Children = new List<IPrintable>() { left, right };
        }
    }

    public class Equality : BinaryAtomicGoal
    {
        public Equality(Term left, Term right) :
            base(left, right, s => s.Unify(left, right), "≡", "equivalent")
        { }

        public override AtomicGoal Negate() => new Disequality(this.LeftOperand, this.RightOperand);
    }

    public class Disequality : BinaryAtomicGoal
    {
        public Disequality(Term left, Term right) :
            base(left, right, s => s.DisUnify(left, right), "╪", "NOT equivalent")
        { }

        public override AtomicGoal Negate() => new Equality(this.LeftOperand, this.RightOperand);
    }
}
