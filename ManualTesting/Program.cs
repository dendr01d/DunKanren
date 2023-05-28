using DunKanren.Goals;

namespace DunKanren
{
    internal class Program
    {
        static Goal Splito(Term coll, Term head, Term tail)
        {
            //return new Conjoint()
            //{
            //    coll != Term.nil,
            //    head != Term.nil,
            //    coll == Cons.Truct(head, tail)
            //};
            return Conso(head, tail, coll);
        }

        static Goal Conso(Term car, Term cdr, Term cons)
        {
            return new Conjoint()
            {
                cons != Term.nil,
                Cons.Truct(car, cdr) == cons
            };
        }

        static Goal Appendo(Term a, Term b, Term c)
        {
            return new Disjoint()
            {
                new Conjoint()
                {
                    a == Term.nil,
                    b == c
                },
                new Conjoint()
                {
                    //new NormalDisjunctive()
                    //{
                    //    { a == Term.nil, b == Term.nil, c == Term.nil },
                    //    { c != Term.nil, new Disjunction(a != Term.nil, b != Term.nil) }
                    //},
                    new CallFresh((first, aRest, cRest) => new Conjoint()
                    {
                        first != Term.nil,
                        a != Term.nil,
                        c != Term.nil,
                        Conso(first, aRest, a),
                        Conso(first, cRest, c),
                        Appendo(aRest, b, cRest)
                    })
                }
            };
        }

        //static Goal Appendo2(Term a, Term b, Term c)
        //{
        //    static Goal helper(Seq seqA, Seq seqB, Seq seqC)
        //    {
        //        return new Disjoint()
        //        {
        //            new Conjoint()
        //            {
        //                seqB.Empty() == Term.True,
        //                seqA == seqC
        //            },
        //            new CallFresh((aMore, bFront, bRest) => new Conjoint()
        //            {
        //                bFront == Seq.PeekFront(seqB),
        //                bRest == Seq.PopFront(seqB),
        //                aMore == Seq.PushBack(seqA, bFront),
        //                Appendo2(aMore, bRest, seqC)
        //            })
        //        };
        //    }

        //    return helper(Seq.Uence(a), Seq.Uence(b), Seq.Uence(c));
        //}

        static Goal Membero(Term a, Term coll)
        {
            return new CallFresh((first, rest) => new Conjoint()
            {
                Splito(coll, first, rest),
                new Disjoint()
                {
                    a == first,
                    Membero(a, rest)
                }
            });
        }

        static Goal NotMembero(Term a, Term coll)
        {
            return Membero(a, coll).Negate();
        }

        static Goal Removeo(Term x, Term collA, Term collB)
        {
            return new Disjoint()
            {
                new Conjoint()
                {
                    collA == Term.nil,
                    collB == Term.nil
                },
                new CallFresh((firstA, restA, firstB, restB) => new Conjoint()
                {
                    Splito(collA, firstA, restA),
                    Splito(collB, firstB, restB),
                    x != firstB,
                    new NormalDisjunctive()
                    {
                        { x == firstA, firstA != firstB, Removeo(x, restA, collB) },
                        { x != firstA, firstA == firstB, Removeo(x, restA, restB) }
                    }
                })
            };
        }

        static Goal Distincto(Term coll)
        {
            return new Disjoint()
            {
                coll == Term.nil,
                new CallFresh((first, rest) => new Conjoint()
                {
                    Splito(coll, first, rest),
                    NotMembero(first, rest),
                    Distincto(rest)
                })
            };
        }

        static Goal Uniqueo(Term coll)
        {
            return new Disjoint()
            {
                coll == Term.nil,
                new CallFresh((first, rest) => new Conjoint()
                {
                    Splito(coll, first, rest),
                    NotMembero(first, rest),
                    Uniqueo(rest)
                })
            };
        }

        static Goal Bijecto(Term collA, Term collB)
        {
            return new Conjoint()
            {
                Uniqueo(collA),
                Uniqueo(collB),
                new Disjoint()
                {
                    new Conjoint()
                    {
                        collA == Term.nil,
                        collB == Term.nil
                    },
                    new CallFresh((firstA, restA, firstB, restB) => new Conjoint()
                    {
                        Splito(collA, firstA, restA),
                        Splito(collB, firstB, restB),
                        new Disjoint()
                        {
                            firstA == firstB,
                            new Conjoint()
                            {
                                Membero(firstA, restB),
                                Membero(firstB, restA)
                            }
                        },
                        Bijecto(restA, restB)
                    })
                }
            };
        }

        //static Goal Bijecto (Term collA, Term collB)
        //{
        //    //let's try another tactic
        //    //there's a bijection if every element in A is unique
        //    //and if B can be rearranged to be equal to A
        //    return new Shell(new Conj()
        //    {
        //        Distincto(collA),
        //        Mutato(collA, collB)
        //    },
        //    "(" + collA.ToString() + " <--> " + collB.ToString() + ")",
        //    "There's a bijective (1-to-1) mapping from " + collA.ToString() + " to " + collB.ToString() + ". As such:"
        //    );

        //}

        ///// <summary>
        ///// Establishes that B is either identical to or a valid reordering of A
        ///// </summary>
        //static Goal Mutato(Term collA, Term collB)
        //{
        //    return new Shell(new Disj()
        //    {
        //        new Conj()
        //        {
        //            collA == Term.nil,
        //            collB == Term.nil,
        //        },
        //        new CallFresh((firstA, restA, lessB) => new Conj()
        //        {
        //            Popo(collA, firstA, restA),
        //            Membero(firstA, collB),
        //            Removeo(firstA, collB, lessB),
        //            NotMembero(firstA, lessB),
        //            Mutato(restA, lessB)
        //        })
        //    },
        //    "(" + collA.ToString() + " <~~> " + collB.ToString() + ")",
        //    collA.ToString() + " can be reordered to form " + collB.ToString() + ". As such:"
        //    );
        //}

        // h e l l o 'n
        // o l l e h 'n

        static Goal Reverseo(Term collA, Term collB)
        {
            static Goal helper(Term coll_a, Term coll_b, Term mem)
            {
                return new Disjoint()
                {
                    new Conjoint()
                    {
                        coll_a == Term.nil,
                        coll_b == mem
                    },
                    new CallFresh((firstA, restA, newB) => new Conjoint()
                    {
                        Splito(coll_a, firstA, restA),
                        newB == Cons.Truct(firstA, mem),
                        helper(restA, coll_b, newB)
                    })
                };
            }

            return helper(collA, collB, Term.nil);
        }

        //static Goal Puzzle()
        //{
        //    return new CallFresh((Aramis, Athos, Pathos, Constance) =>
        //        new CallFresh((Aramis_P, Athos_P, Pathos_P, Constance_P, hotel, jardin, estate) =>
        //            new CallFresh((Aramis_A, Athos_A, Pathos_A, Constance_A, musket, duel, rendM, rendF) => new Conjoint()
        //            {
        //                hotel == ValueFactory.Sym("Hotel Treville"),
        //                jardin == ValueFactory.Sym("Jardin du Luxembourg"),
        //                estate == ValueFactory.Sym("Geroge Villier's Estate"),

        //                musket == ValueFactory.Sym("Cleaning his musket"),
        //                duel == ValueFactory.Sym("Dueling Cardinal Richelieu"),
        //                rendF == ValueFactory.Sym("Meeting with a man"),
        //                rendM == ValueFactory.Sym("Meeting with a woman"),

        //                Aramis == Cons.Truct(Aramis_P, Aramis_A),
        //                Athos == Cons.Truct(Athos_P, Athos_A),
        //                Pathos == Cons.Truct(Pathos_P, Pathos_A),
        //                Constance == Cons.Truct(Constance_P, Constance_A),

        //                new Conjoint()
        //                {
        //                    Membero(Aramis_P, Cons.TructList(hotel, jardin, estate)),
        //                    Membero(Athos_P, Cons.TructList(hotel, jardin, estate)),
        //                    Membero(Pathos_P, Cons.TructList(hotel, jardin, estate)),
        //                    Membero(Constance_P, Cons.TructList(hotel, jardin, estate))
        //                },

        //                new Conjoint()
        //                {
        //                    Membero(Aramis_A, Cons.TructList(musket, duel, rendM)),
        //                    Membero(Athos_A, Cons.TructList(musket, duel, rendM)),
        //                    Membero(Pathos_A, Cons.TructList(musket, duel, rendM)),
        //                    Constance_A == rendF
        //                },

        //                new Conjoint()
        //                {
        //                    Aramis_P != Athos_P,
        //                    Athos_P != Pathos_P,
        //                    Pathos_P != Aramis_P
        //                },

        //                new Conjoint()
        //                {
        //                    Aramis_A != Athos_A,
        //                    Athos_A != Pathos_A,
        //                    Pathos_A != Aramis_A
        //                },

        //                //setup complete

        //                new Disjoint()
        //                {
        //                    new Conjoint()
        //                    {
        //                        Aramis_P != jardin,
        //                        Athos != jardin
        //                    },
        //                    new Disjoint()
        //                    {
        //                        Aramis == Cons.Truct(hotel, musket),
        //                        Athos == Cons.Truct(hotel, musket),
        //                        Pathos == Cons.Truct(hotel, musket)
        //                    }
        //                },

        //                new Conjoint()
        //                {
        //                    Pathos_A != duel,
        //                    new Conjoint()
        //                    {
        //                        Aramis != Cons.Truct(jardin, duel),
        //                        Athos != Cons.Truct(jardin, duel),
        //                        Pathos != Cons.Truct(jardin, duel)
        //                    }
        //                },

        //                new Conjoint()
        //                {
        //                    Aramis_A != musket,
        //                    Aramis_P != hotel
        //                },

        //                new Disjoint()
        //                {
        //                    Pathos_A != musket,
        //                    Constance_P != jardin
        //                },

        //                new Conjoint()
        //                {
        //                    new Disjoint()
        //                    {
        //                        Pathos_P != estate,
        //                        new Disjoint()
        //                        {
        //                            Aramis == Cons.Truct(estate, duel),
        //                            Athos == Cons.Truct(estate, duel),
        //                            Pathos == Cons.Truct(estate, duel)
        //                        }
        //                    },
        //                    new Disjoint()
        //                    {
        //                        new Conjoint()
        //                        {
        //                            Aramis != Cons.Truct(estate, duel),
        //                            Athos != Cons.Truct(estate, duel),
        //                            Pathos != Cons.Truct(estate, duel)
        //                        },
        //                        Pathos_P == estate
        //                    }
        //                },

        //                Constance_P != hotel
        //            })));
        //}

        //static Goal PuzzleNew()
        //{
        //    return new CallFresh((Aramis, Athos, Pathos, Constance) =>
        //        new CallFresh((Aramis_Doing, Athos_Doing, Pathos_Doing, Constance_Doing, musket, rendezvous, duel) =>
        //            new CallFresh((Aramis_Location, Athos_Location, Pathos_Location, Constance_Location, hotel, jardin, estate) => new Conjoint()
        //            {
        //                hotel == "Hotel Treville",
        //                jardin == "Jardin du Luxembourg",
        //                estate == "George Villier's estate",

        //                musket == "Cleaning his musket",
        //                rendezvous == "Having a rendezvous",
        //                duel == "Dueling Cardinal Richelieu",

        //                Constance_Doing == rendezvous,

        //                Aramis == Cons.Truct(Aramis_Doing, Aramis_Location),
        //                Athos == Cons.Truct(Athos_Doing, Athos_Location),
        //                Pathos == Cons.Truct(Pathos_Doing, Pathos_Location),
        //                Constance == Cons.Truct(Constance_Doing, Constance_Location),

        //                Bijecto(Cons.TructList(Aramis_Doing, Athos_Doing, Pathos_Doing), Cons.TructList(musket, rendezvous, duel)),
        //                Bijecto(Cons.TructList(Aramis_Location, Athos_Location, Pathos_Location), Cons.TructList(hotel, jardin, estate)),

        //                new Implication
        //                (
        //                    new Disjoint()
        //                    {
        //                        Aramis_Location == jardin,
        //                        Athos_Location == jardin
        //                    },
        //                    new NormalDisjunctive()
        //                    {
        //                        { Aramis_Doing == musket, Aramis_Location == hotel },
        //                        { Athos_Doing == musket, Athos_Location == hotel },
        //                        { Pathos_Doing == musket, Pathos_Location == hotel }
        //                    }
        //                ),

        //                Pathos_Doing != duel,

        //                new NormalDisjunctive()
        //                {
        //                    { Aramis_Doing == duel, Aramis_Location == jardin },
        //                    { Athos_Doing == duel, Athos_Location == jardin },
        //                    { Pathos_Doing == duel, Pathos_Location == jardin }
        //                }.Negate(),

        //                Aramis_Doing != musket,
        //                Aramis_Location != hotel,

        //                new Implication(Pathos_Doing == musket, Constance_Location != jardin),
        //                new BiImplication(
        //                    new NormalDisjunctive()
        //                    {
        //                        { Aramis_Doing == duel, Aramis_Location == estate },
        //                        { Athos_Doing == duel, Athos_Location == estate },
        //                        { Pathos_Doing == duel, Pathos_Location == estate }
        //                    },
        //                    Pathos_Location == estate
        //                ),

        //                Constance_Location != hotel
        //            })));
        //}

        static void Main()
        {

            //var g3 = Goal.CallFresh(x => Goal.CallFresh(y => Goal.Conjunction(Goal.Equality(x, 5), Goal.Equality(x, y))));


            //Goal g = Goal.CallFresh(x => Goal.CallFresh(y => Goal.Conjunction(Goal.Equality(x, 5), Goal.Disjunction(Goal.Equality(y, x), Goal.Equality(y, 6)))));
            /*
            Goal g = new CallFresh((x, y) => new Conj()
            {
                x == 5,
                new Disj()
                {
                    y == x,
                    y == 6
                }
            });
            */
            //Goal g = new CallFresh(x => Appendo("hello", x, "hello world!"));

            //Goal g = new CallFresh((x, y) => Appendo(x, y, "hello world!"));

            //Goal g = new CallFresh(x => Membero("a", x));


            //Goal g = new CallFresh((x, y) => new Conjoint()
            //{
            //    new Disjoint()
            //    {
            //        x == 5,
            //        x == 6
            //    },
            //    y == x,
            //    y != 5
            //});


            //Goal g = PuzzleNew();

            //Goal g = new CallFresh((x, y) => new Conjunction(x == 5, y == 6));

            //Goal g = new CallFresh((x, y) => Appendo(x, y, "abc"));

            Goal g = new CallFresh((x, y, z, w) => new Conjunction(Appendo(x, y, z), Appendo(z, w, "abcd")));

            Console.WriteLine("Sphinx of black quartz! Judge my vow:");
            Console.WriteLine();
            Console.WriteLine(IO.Graft(g.ToTree()));

            int answers = 0;

            using (var iter = g.Pursue().GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("     Answer #" + (answers + 1).ToString());
                    Console.WriteLine("--------------------------------------------------\n");
                    Console.WriteLine(iter.Current.ToString(1));
                    //Console.WriteLine(String.Join("\n", iter.Current.Subs.Select(x => $"{x.Key}: {x.Value}")));
                    Console.WriteLine();
                    //Console.WriteLine(IO.Graft(g.ToTree()));
                    ++answers;

                    IO.Prompt(true);
                }
            }

            Console.WriteLine("--------------------------------------------------\n");
            //Console.WriteLine(IO.Graft(g.ToTree()));

            if (answers == 0)
            {
                Console.WriteLine("The sphinx has judged your vow and found it wanting...");
            }
            else
            {
                Console.WriteLine("Your vow has been judged, so says the sphinx of black quartz!");
            }
            Console.WriteLine("...");
            Console.ReadKey(true);
        }
    }
}




