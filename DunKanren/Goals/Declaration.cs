using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    /// <summary>
    /// A goal that essentially acts as a pass-through, but it adds new variables to the pool
    /// </summary>
    public sealed class Fresh : Goal
    {
        public override string Expression => $"£ ({String.Join(", ", this.VariableNames)})";
        public override string Description => $"There exist variables ({String.Join(", ", this.VariableNames)})";
        public override IEnumerable<IPrintable> Components => Array.Empty<IPrintable>();

        public readonly string[] VariableNames;

        public Fresh(params string[] newVars)
        {
            this.VariableNames = newVars;
        }

        public Fresh(IEnumerable<System.Reflection.ParameterInfo> newVars)
        {
            this.VariableNames = newVars.Select(x => x.Name ?? "UnkVar").ToArray();
        }

        protected override Stream Pursuit(State s)
        {
            s.DeclareVars(out State newState, this.VariableNames);
            return Stream.Singleton(newState);
        }

        public Stream PursueIn(State s, out Variable[] vars)
        {
            vars = s.DeclareVars(out State newState, this.VariableNames);
            return Stream.Singleton(newState);
        }

        public override Goal Negate()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class Call : Goal
    {
        public override string Expression => $"Lambda()";
        public override string Description => $"The evaluation of the lambda expression takes the form";
        public override IEnumerable<IPrintable> Components => Array.Empty<IPrintable>();

        private readonly Func<State, Stream> Application;

        public Call(Func<State, Stream> lambda)
        {
            this.Application = lambda;
        }

        public Call(Goal lambda)
        {
            this.Application = lambda.PursueIn;
        }

        protected override Stream Pursuit(State s)
        {
            return this.Application(s.Next());
        }

        public override Goal Negate()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class CallFresh : Goal
    {
        public override string Expression => this.DynamicExpression;
        public override string Description => this.DynamicDescription;
        public override IEnumerable<IPrintable> Components => this.DynamicChild;

        private string DynamicExpression;
        private string DynamicDescription;
        private IEnumerable<IPrintable> DynamicChild = Array.Empty<IPrintable>();

        private string[] Declarations => this.Freshener.VariableNames;
        private Fresh Freshener;
        private Call Caller;

        protected override Stream Pursuit(State s)
        {
            return this.Caller.PursueIn(s);
        }

        public override Goal Negate()
        {
            throw new NotImplementedException();
        }

        private CallFresh(System.Reflection.MethodInfo body, Func<Variable[], Goal> constructor)
        {
            this.Freshener = new Fresh(body.GetParameters());
            this.Caller = new Call((State s) =>
            {
                Stream str = this.Freshener.PursueIn(s, out Variable[] newVars);
                Goal inner = constructor(newVars);

                this.DynamicExpression = $"ƒ({String.Join(", ", this.Declarations)})";
                this.DynamicDescription = $"Lambda on ({String.Join(", ", this.Declarations)})";
                this.DynamicChild = new IPrintable[] { inner };

                return Stream.New(str.SelectMany(inner.PursueIn));
            });

            this.DynamicExpression = $"ƒ(?)";
            this.DynamicDescription = $"Unevaluated Lambda";
            //this.DynamicChild = this.Caller;
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
    }



}
