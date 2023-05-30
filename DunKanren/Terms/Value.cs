﻿using System;
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

        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs<O>(State s, Value<O> other) => other.Data!.Equals(this.Data);

        public override bool TryUnifyWith(State s, Term other, out State result) => other.TryUnifyWith(s, this, out result);
        public override bool TryUnifyWith<O>(State s, Value<O> other, out State result) =>
            this.SameAs(s, other)
            ? s.Affirm(other, this, out result)
            : s.Reject(other, this, out result);

        public override string ToString()
        {
            if (this.Data is char c)
            {
                return $"\'{c}\'";
            }
            else if (this.Data != null && this.Data.ToString() is string s)
            {
                return s;
            }
            else
            {
                return "<UNK?>";
            }
        }
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
