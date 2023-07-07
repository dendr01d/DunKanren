using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    //public interface INumericTerm<N> where N : INumber<N>
    //{
    //    N RawValue { get; init; }

    //    P BinaryOperation<O, P>(Func<N, O, P> op, INumericTerm<O> other)
    //        where O : INumber<O>
    //        where P : INumber<P>
    //    {
    //        return other.DispatchOperation<P>(this.Curry(op));
    //    }

    //    P DispatchOperation<P>(Func<N, P> op)
    //        where P : INumber<P>
    //    {
    //        return op(this.RawValue);
    //    }

    //    Func<O, P> Curry<O, P>(Func<N, O, P> op)
    //        where O : INumber<O>
    //        where P : INumber<P>
    //    {
    //        return (O other) => op(this.RawValue, other);
    //    }
    //}

    public class Number : Term
    {
        public int Numerator { get; private set; }
        public int Denominator { get; private set; }

        public double AsDecimal => this.Numerator / this.Denominator;

        public Number(Number n) : this(n.Numerator, n.Denominator) { }

        public Number(int num)
        {
            this.Denominator = num < 0 ? -1 : 1;
            this.Numerator = int.Abs(num);
            this.Simplify();
        }

        public Number(int num, int denom) : this(num)
        {
            this.Denominator *= denom;
            this.Simplify();
        }

        const int Max_Decimals = 10;

        public Number(double decim)
        {
            int denom = decim < 0 ? -1 : 1;
            decim = double.Abs(decim);
            decim = double.Round(decim, Max_Decimals);

            while (decim - double.Round(decim) != 0)
            {
                decim *= 10;
                denom *= 10;
            }

            this.Numerator = (int)decim;
            this.Denominator = denom;
        }

        //-------------------------

        //simplify the number's internal fraction as much as possible
        private void Simplify()
        {
            //https://en.wikipedia.org/wiki/Euclidean_algorithm
            //recursive algorithm for finding the greatest common divisor between two numbers
            static int EuclideanAlgorithm(int a, int b)
            {
                if (a == 0) return b;
                else if (b == 0) return a;
                else
                {
                    if (a > b) return EuclideanAlgorithm(b, a % b);
                    else return EuclideanAlgorithm(a, b % a);
                }
            }

            int gcd = EuclideanAlgorithm(this.Numerator, int.Abs(this.Denominator));

            this.Numerator /= gcd;
            this.Denominator /= gcd;
        }

        //-------------------------

        public override bool TermEquals(State s, Term other) => other.TermEquals(s, this);
        public override bool TermEquals(State s, Number other) => this.Numerator == other.Numerator && this.Denominator == other.Denominator;

        public override bool TryUnifyWith(State s, Term other, out State result) => other.TryUnifyWith(s, this, out result);
        public override bool TryUnifyWith(State s, Number other, out State result) =>
            this.TermEquals(s, other)
            ? s.Affirm(other, this, out result)
            : s.Reject(other, this, out result);

        public override string ToString()
        {
            if (this.Denominator == 1)
            {
                return this.Numerator.ToString();
            }
            else if (this.Denominator % 10 == 0)
            {
                return this.AsDecimal.ToString();
            }
            else
            {
                return $"{this.Numerator}/{this.Denominator}";
            }
        }
    }
}
