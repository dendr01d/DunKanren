using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    public static class StdGoals
    {
        public static Goal Assert(bool statement)
        {
            return statement ? new Top() : new Bottom();
        }

        public static Goal Conso(Term car, Term cdr, Term cons)
        {
            return cdr is Nil
                ? car == cons
                : Cons.Truct(car, cdr) == cons;
        }

        public static Goal Appendo(Term a, Term b, Term c)
        {
            return new Disj()
            {
                new Conj()
                {
                    a == Term.NIL,
                    b == c
                },
                new Conj()
                {
                    new CallFresh((first, aRest, cRest) => new Conj()
                    {
                        Cons.Truct(first, aRest) == a,
                        Cons.Truct(first, cRest) == c,
                        Appendo(aRest, b, cRest)
                    })
                }
            };
        }

        public static Goal Membero(Term a, Term coll)
        {
            return new CallFresh((first, rest) => new Conj()
            {
                Conso(first, rest, coll),
                new Disj()
                {
                    Membero(a, rest),
                    a == first
                }
            });
        }

        public static Goal NotMembero(Term a, Term coll)
        {
            return new Disj()
            {
                coll == Term.NIL,
                new CallFresh((first, rest) => new Conj()
                {
                    Conso(first, rest, coll),
                    first != a,
                    a != coll,
                    NotMembero(a, rest)
                })
            };
        }

        public static Goal Removeo(Term r, Term collWith, Term collWithout)
        {
            return new Disj()
            {
                Goal.AND(collWith == Term.NIL, collWithout == Term.NIL),
                new CallFresh((firstWith, restWith, firstWithout, restWithout) => new Conj()
                {
                    Conso(firstWith, restWith, collWith),
                    Conso(firstWithout, restWithout, collWithout),
                    r != firstWithout,
                    new DNF()
                    {
                        { r == firstWith, Removeo(r, restWith, collWithout) },
                        { firstWith == firstWithout, Removeo(r, restWith, restWithout) }
                    }
                })
            };
        }

        public static Goal Distincto(Term coll)
        {
            return new Disj()
            {
                coll == Term.NIL,
                new CallFresh((first, rest) => new Conj()
                {
                    Conso(first, rest, coll),
                    NotMembero(first, rest),
                    Distincto(rest)
                })
            };
        }

        //static Goal Sharedo(Term collA, Term collB)
        //{
        //    return new Disj()
        //    {
        //        collA == collB,
        //        new CallFresh((firstA, restA, firstB, restB) => new Conj()
        //        {
        //            Conso(firstA, restA, collA),
        //            Conso(firstB, restB, collB),
        //            new Disj()
        //            {
        //                firstA == firstB,
        //                new Conj()
        //                {
        //                    Membero(firstA, restB),
        //                    Membero(firstB, restA)
        //                }
        //            },
        //            Sharedo(restA, restB)
        //        })
        //    };
        //}

        public static Goal Lengtho(Term coll, Term length)
        {
            Goal helper(Term _coll, Term _length, Term count)
            {
                return new Disj()
                {
                    Goal.AND(_coll == Term.NIL, _length == count),
                    new CallFresh((collFirst, collRest, inc) => new Conj()
                    {
                        Conso(collFirst, collRest, _coll),
                        Conso(Term.NIL, count, inc),
                        helper(collRest, _length, inc)
                    })
                };
            }

            return helper(coll, length, Cons.Truct(0));
        }

        public static Goal Bijecto(Term collA, Term collB)
        {
            //there's a 1-to-1 correspondence between the two sets A and B if:
            //A and B are the same length
            //A's elements are distinct
            //B's elements are distinct
            //each of A's elements appears in B

            Goal helper(Term _collA, Term _collB)
            {
                return new Disj()
                {
                    _collA == Term.NIL,
                    new CallFresh((firstA, restA) => new Conj()
                    {
                        Conso(firstA, restA, _collA),
                        Membero(firstA, collB),
                        helper(restA, _collB)
                    })
                };
            }

            return new Conj()
            {
                Distincto(collA),
                Distincto(collB),
                helper(collA, collB)
            };
        }

        static Goal Bijecto_old(Term collA, Term collB)
        {
            Goal helper(Term _collA, Term _collB)
            {
                return new Disj()
                {
                    _collA == _collB,
                    new CallFresh((firstA, restA, firstB, restB) => new Conj()
                    {
                        Conso(firstA, restA, _collA),
                        Conso(firstB, restB, _collB),
                        new Disj()
                        {
                            firstA == firstB,
                            new Conj()
                            {
                                firstA != firstB,
                                Membero(firstA, collB),
                                Membero(firstB, collA)
                            }
                        },
                        helper(restA, restB)
                    })
                };
            }

            return new Conj()
            {
                Distincto(collA),
                Distincto(collB),
                helper(collA, collB)
            };
        }

        // h e l l o 'n
        // o l l e h 'n

        static Goal Reverseo(Term collA, Term collB)
        {
            static Goal helper(Term coll_a, Term coll_b, Term mem)
            {
                return new Disj()
                {
                    new Conj()
                    {
                        coll_a == Term.NIL,
                        coll_b == mem
                    },
                    new CallFresh((firstA, restA, newMem) => new Conj()
                    {
                        Conso(firstA, restA, coll_a),
                        newMem == Cons.Truct(firstA, mem),
                        helper(restA, coll_b, newMem)
                    })
                };
            }

            return helper(collA, collB, Term.NIL);
        }
    }
}
