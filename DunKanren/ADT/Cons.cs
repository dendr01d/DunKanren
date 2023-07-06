using System.Collections.Generic;

namespace DunKanren.ADT
{
    public abstract partial class Term
    {
        public abstract partial class Cons : Term
        {
            public abstract Term Car { get; }
            public abstract Term Cdr { get; }

            public sealed class Pair<T1, T2> : Cons
                where T1 : Term
                where T2 : Term
            {
                private T1 _car;
                private T2 _cdr;

                public override Term Car => _car;
                public override Term Cdr => _cdr;

                public Pair(T1 car, T2 cdr)
                {
                    _car = car;
                    _cdr = cdr;
                }

                public override string ToString()
                {
                    return $"({_car} . {_cdr})";
                }
            }

            public abstract class List<T> : Cons
                where T : Term
            {
                public sealed class ListNode<LItem, LTail> : List<LItem>
                    where LItem : Term
                    where LTail : List<LItem>
                {
                    private LItem _car;
                    private LTail _cdr;

                    public override Term Car => _car;
                    public override Term Cdr => _cdr;

                    public ListNode(LItem car, LTail cdr)
                    {
                        _car = car;
                        _cdr = cdr;
                    }

                    public override string ToString()
                    {
                        return $"{_car}, {_cdr}";
                    }
                }

                public sealed class ListTail<LT> : List<LT>
                    where LT : Term
                {
                    private LT _car;
                    private Nil _cdr;

                    public override Term Car => _car;
                    public override Term Cdr => _cdr;

                    public ListTail(LT car)
                    {
                        _car = car;
                        _cdr = Nil.Value;
                    }

                    public override string ToString()
                    {
                        return $"{_car}";
                    }
                }
            }
        }
    }
}
