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
        private readonly D _data;

        public D GetData() => _data;

        public Value(D data)
        {
            this._data = data;
        }

        public override bool Equals(Term? other) => other is Value<D> t && t._data != null && t._data.Equals(_data);

        public override string ToString() => this._data?.ToString() ?? "<UNK?>";
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
