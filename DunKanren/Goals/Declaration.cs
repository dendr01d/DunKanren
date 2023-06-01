using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    public abstract class Declaration : Goal
    {
        public override string Expression => $"£ ({String.Join(", ", this.VariableNames)})";
        public override string Description => $"There exist variables ({String.Join(", ", this.VariableNames)})";
        public override IEnumerable<IPrintable> ChildGoals => Array.Empty<IPrintable>();

        public readonly string[] VariableNames;

        protected Declaration(params string[] newVars)
        {
            this.VariableNames = newVars;
        }

        protected Declaration(IEnumerable<System.Reflection.ParameterInfo> newVars)
        {
            this.VariableNames = newVars.Select(x => x.Name ?? "UnkVar").ToArray();
        }
    }

    /// <summary>
    /// A goal that essentially acts as a pass-through, but it adds new variables to the pool
    /// </summary>
    public sealed class Fresh : Declaration
    {

        public Fresh(params string[] newVars) : base(newVars) { }

        public Fresh(IEnumerable<System.Reflection.ParameterInfo> newVars) : base(newVars) { }

        protected override Stream Application(State s)
        {
            s.DeclareVars(out State newState, this.VariableNames);
            return Stream.Singleton(newState);
        }

        public override Goal Negate()
        {
            return new Top();
        }

        public override int Ungroundedness => this.VariableNames.Length;
    }

    public sealed class CallFresh : Declaration
    {
        public override string Expression => this.DynamicExpression;
        public override string Description => this.DynamicDescription;
        public override IEnumerable<IPrintable> ChildGoals => this.DynamicChild;

        private string DynamicExpression;
        private string DynamicDescription;
        private IEnumerable<IPrintable> DynamicChild = Array.Empty<IPrintable>();

        private Func<State, Stream> ApplicationFunc;

        protected override Stream Application(State s)
        {
            return this.ApplicationFunc(s);
        }

        public override Goal Negate()
        {
            return new Top();
        }

        private CallFresh(System.Reflection.MethodInfo body, Func<Variable[], Goal> constructor) : base(body.GetParameters())
        {
            //string varNames = String.Join(", ", body.GetParameters().Select(x => x.Name));
            this.DynamicExpression = $"ƒ(?)";
            this.DynamicDescription = $"Unevaluated Lambda";

            this.ApplicationFunc = (State s) =>
            {
                Variable[] newVars = s.DeclareVars(out State newState, this.VariableNames);
                Goal newGoal = constructor(newVars);

                this.DynamicExpression = $"ƒ({String.Join(", ", this.VariableNames)})";
                this.DynamicDescription = $"Lambda on ({String.Join(", ", this.VariableNames)})";

                return newGoal.PursueIn(newState);
            };
        }

        public CallFresh(Func<Goal> lambda) :
            this(lambda.Method, (v) => lambda())
        { }

        public CallFresh(Func<Variable, Goal> lambda) :
            this(lambda.Method, (v) => lambda(v[0]))
        { }

        public CallFresh(Func<Variable, Variable, Goal> lambda) :
            this(lambda.Method, (v) => lambda(v[0], v[1]))
        { }

        public CallFresh(Func<Variable, Variable, Variable, Goal> lambda) :
            this(lambda.Method, (v) => lambda(v[0], v[1], v[2]))
        { }

        public CallFresh(Func<Variable, Variable, Variable, Variable, Goal> lambda) :
            this(lambda.Method, (v) => lambda(v[0], v[1], v[2], v[3]))
        { }

        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Goal> lambda) :
            this(lambda.Method, (v) => lambda(v[0], v[1], v[2], v[3], v[4]))
        { }

        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) :
            this(lambda.Method, (v) => lambda(v[0], v[1], v[2], v[3], v[4], v[5]))
        { }

        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) :
            this(lambda.Method, (v) => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6]))
        { }

        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) :
            this(lambda.Method, (v) => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7]))
        { }

        public override int Ungroundedness => -1 * this.VariableNames.Length;
    }



}
