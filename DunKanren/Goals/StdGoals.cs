using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    public static class StdGoals
    {
        public static Goal Conso(Term car, Term cdr, Term cons)
        {
            return new Conj()
            {
                Cons.Truct(car, cdr) == cons
            };
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
                    c != Term.NIL,
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
                    a == first,
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
                    NotMembero(a, rest)
                })
            };
        }

        static Goal Removeo(Term r, Term collFull, Term collPrun)
        {
            return new Disj()
            {
                Goal.AND(collFull == Term.NIL, collPrun == Term.NIL),
                new CallFresh((firstF, restF, firstP, restP) => new Conj()
                {
                    Conso(firstF, restF, collFull),
                    Conso(firstP, restP, collPrun),
                    r != firstP,
                    new DNF()
                    {
                        { r == firstF, Removeo(r, restF, collPrun) },
                        { firstF == firstP, Removeo(r, restF, restP) }
                    }
                })
            };
        }

        static Goal Distincto(Term coll)
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

        static Goal Sharedo(Term collA, Term collB)
        {
            return new Disj()
            {
                collA == collB,
                new CallFresh((firstA, restA, firstB, restB) => new Conj()
                {
                    Conso(firstA, restA, collA),
                    Conso(firstB, restB, collB),
                    new Disj()
                    {
                        firstA == firstB,
                        new Conj()
                        {
                            Membero(firstA, restB),
                            Membero(firstB, restA)
                        }
                    },
                    Sharedo(restA, restB)
                })
            };
        }

        static Goal Bijecto(Term collA, Term collB)
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
