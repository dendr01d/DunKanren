using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public class Value<D> : Term
    {
        private readonly D Data;

        public Value(D data)
        {
            this.Data = data;
        }


        public Term Reflect() => this;
        public bool TryReify(out D? result)
        {
            result = this.Data;
            return true;
        }


        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs<O>(State s, Value<O> other) => other.Data!.Equals(this.Data);

        public override State? UnifyWith(State s, Term other) => other.UnifyWith(s, this);
        public override State? UnifyWith<O>(State s, Value<O> other) => this.SameAs(s, other) ? s.Affirm(other, this) : s.Reject(other, this);



        //other.Equals(this) ? s.Affirm(other, this) : s.Reject(other, this);

        public override string ToString() => this.Data?.ToString() ?? "NULL";
    }

    public static class ValueFactory
    {
        public static Value<D> Box<D>(D data) => new(data);

        public static Value<string> Sym(string s) => new Value<string>(s);
    }


    /*
    public abstract class MetaTerm<D, T> : Term where T : Term
    {
        public readonly T Literal;

        public MetaTerm(T t)
        {
            this.Literal = t;
        }

        public override string ToString() => "<" + typeof(D).Name + "> " + this.Literal.ToString();
    }

    public class MetaValue<D> : MetaTerm<D, Value<D>>
    {
        public MetaValue(Value<D> v) : base(v) { }

        public override bool Equals(State s, Term other) => other.Equals(s, this.Literal);

        public override State? UnifyWith(State s, Term other) => s.Reject(other, this);
        public override State? UnifyWith<D>(State s, Terms.MetaTerm<D> other)
        {
            return base.UnifyWith<D>(s, other);
        }
    }
    */

}
