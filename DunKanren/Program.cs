using DunKanren.Terms;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DunKanren
{
    internal class Program
    {
        static Goal Popo(Term coll, Term head, Term tail)
        {
            return new Conj()
            {
                coll != Term.nil,
                coll == Cons.Truct(head, tail)
            };
        }

        static Goal Appendo(Term a, Term b, Term c)
        {
            return new Shell(new Disj()
            {
                new Conj()
                {
                    a == Term.nil,
                    b == c
                },
                new CallFresh((first, aRest, cRest) => new Conj()
                {
                    Popo(a, first, aRest),
                    Popo(c, first, cRest),
                    Appendo(aRest, b, cRest)
                })
            },
            "(" + a.ToString() + " | " + b.ToString() + ") == " + c.ToString(),
            a.ToString() + " appended by " + b.ToString() + " is " + c.ToString() + ". As such:"
            );
        }

        static Goal Membero(Term a, Term coll)
        {
            return new Shell(new Disj()
            {
                new CallFresh((first, rest) => new Conj()
                {
                    Popo(coll, first, rest),
                    new Disj()
                    {
                        a == first,
                        Membero(a, rest)
                    }
                })
            },
            "(" + a.ToString() + " ε " + coll.ToString() + ")",
            a.ToString() + " is an element of " + coll.ToString() + ". As such:"
            );
        }

        static Goal NotMembero(Term a, Term coll)
        {
            return new Shell(new Disj()
            {
                coll == Term.nil,
                new CallFresh((first, rest) => new Conj()
                {
                    Popo(coll, first, rest),
                    a != first,
                    NotMembero(a, rest)
                })
            },
            "(" + a.ToString() + " !ε " + coll.ToString() + ")",
            a.ToString() + " is NOT an element of " + coll.ToString() + ". As such:"
            );
        }

        static Goal Removeo(Term x, Term collA, Term collB)
        {
            return new Shell(new Disj()
            {
                new Conj()
                {
                    collA == Term.nil,
                    collB == Term.nil
                },
                new CallFresh((firstA, restA, firstB, restB) => new Conj()
                {
                    Popo(collA, firstA, restA),
                    Popo(collB, firstB, restB),
                    x != firstB,
                    new DNF()
                    {
                        { x == firstA, firstA != firstB, Removeo(x, restA, collB) },
                        { x != firstA, firstA == firstB, Removeo(x, restA, restB) }
                    }
                })
            },
            "(" + collA.ToString() + " sans " + x.ToString() + ") == " + collB.ToString(),
            collA.ToString() + " without " + x.ToString() + " is " + collB.ToString() + ". As such:"
            );
        }

        static Goal Distincto(Term coll)
        {
            return new Shell(new Disj()
            {
                coll == Term.nil,
                new CallFresh((first, rest) => new Conj()
                {
                    Popo(coll, first, rest),
                    NotMembero(first, rest),
                    Distincto(rest)
                })
            },
            "│≥ (" + coll.ToString() + ")",
            "The elements in " + coll.ToString() + " are distinct and unique. As such:"
            );
        }

        static Goal Bijecto2 (Term collA, Term collB)
        {
            return new Shell(new Conj() {
                //Uniqueo(collA),
                //Uniqueo(collB),
                new Disj()
                {
                    new Conj()
                    {
                        collA == Term.nil,
                        collB == Term.nil,
                    },
                    new CallFresh((firstA, restA) => new Conj()
                    {
                        Popo(collA, firstA, restA),
                        Membero(firstA, collB),
                        new CallFresh(collBP => new Conj()
                        {
                            Removeo(firstA, collB, collBP),
                            //NotMembero(firstA, collBP), //this should be a natural byproduct of each collection being distinctly membered

                            Bijecto(restA, collBP)
                        })
                    })
                }
            },
            "(" + collA.ToString() + " <--> " + collB.ToString() + ")",
            "There's a bijective (1-to-1) mapping from " + collA.ToString() + " to " + collB.ToString() + ". As such:"
            );
        }

        static Goal Bijecto (Term collA, Term collB)
        {
            //let's try another tactic
            //there's a bijection if every element in A is unique
            //and if B can be rearranged to be equal to A
            return new Shell(new Conj()
            {
                Distincto(collA),
                Mutato(collA, collB)
            },
            "(" + collA.ToString() + " <--> " + collB.ToString() + ")",
            "There's a bijective (1-to-1) mapping from " + collA.ToString() + " to " + collB.ToString() + ". As such:"
            );

        }

        /// <summary>
        /// Establishes that B is either identical to or a valid reordering of A
        /// </summary>
        static Goal Mutato(Term collA, Term collB)
        {
            return new Shell(new Disj()
            {
                new Conj()
                {
                    collA == Term.nil,
                    collB == Term.nil,
                },
                new CallFresh((firstA, restA, lessB) => new Conj()
                {
                    Popo(collA, firstA, restA),
                    Membero(firstA, collB),
                    Removeo(firstA, collB, lessB),
                    NotMembero(firstA, lessB),
                    Mutato(restA, lessB)
                })
            },
            "(" + collA.ToString() + " <~~> " + collB.ToString() + ")",
            collA.ToString() + " can be reordered to form " + collB.ToString() + ". As such:"
            );
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
                        coll_a == Term.nil,
                        coll_b == mem
                    },
                    new CallFresh((firstA, restA, newB) => new Conj()
                    {
                        Popo(coll_a, firstA, restA),
                        newB == Cons.Truct(firstA, mem),
                        helper(restA, coll_b, newB)
                    })
                };
            }

            return new Shell(helper(collA, collB, Term.nil),
            "(" + collA.ToString() + " <|> " + collB.ToString() + ")",
            collA.ToString() + " backward is " + collB.ToString() + ". As such:"
            );
        }

        static Goal Puzzle()
        {
            return new CallFresh((Aramis, Athos, Pathos, Constance) =>
                new CallFresh((Aramis_P, Athos_P, Pathos_P, Constance_P, hotel, jardin, estate) =>
                    new CallFresh((Aramis_A, Athos_A, Pathos_A, Constance_A, musket, duel, rendM, rendF) => new Conj()
                    {
                        hotel == ValueFactory.Sym("Hotel Treville"),
                        jardin == ValueFactory.Sym("Jardin du Luxembourg"),
                        estate == ValueFactory.Sym("Geroge Villier's Estate"),

                        musket == ValueFactory.Sym("Cleaning his musket"),
                        duel == ValueFactory.Sym("Dueling Cardinal Richelieu"),
                        rendF == ValueFactory.Sym("Meeting with a man"),
                        rendM == ValueFactory.Sym("Meeting with a woman"),

                        Aramis == Cons.Truct(Aramis_P, Aramis_A),
                        Athos == Cons.Truct(Athos_P, Athos_A),
                        Pathos == Cons.Truct(Pathos_P, Pathos_A),
                        Constance == Cons.Truct(Constance_P, Constance_A),

                        new Conj()
                        {
                            Membero(Aramis_P, Cons.TructList(hotel, jardin, estate)),
                            Membero(Athos_P, Cons.TructList(hotel, jardin, estate)),
                            Membero(Pathos_P, Cons.TructList(hotel, jardin, estate)),
                            Membero(Constance_P, Cons.TructList(hotel, jardin, estate))
                        },

                        new Conj()
                        {
                            Membero(Aramis_A, Cons.TructList(musket, duel, rendM)),
                            Membero(Athos_A, Cons.TructList(musket, duel, rendM)),
                            Membero(Pathos_A, Cons.TructList(musket, duel, rendM)),
                            Constance_A == rendF
                        },

                        new Conj()
                        {
                            Aramis_P != Athos_P,
                            Athos_P != Pathos_P,
                            Pathos_P != Aramis_P
                        },

                        new Conj()
                        {
                            Aramis_A != Athos_A,
                            Athos_A != Pathos_A,
                            Pathos_A != Aramis_A
                        },

                        //setup complete

                        new Disj()
                        {
                            new Conj()
                            {
                                Aramis_P != jardin,
                                Athos != jardin
                            },
                            new Disj()
                            {
                                Aramis == Cons.Truct(hotel, musket),
                                Athos == Cons.Truct(hotel, musket),
                                Pathos == Cons.Truct(hotel, musket)
                            }
                        },

                        new Conj()
                        {
                            Pathos_A != duel,
                            new Conj()
                            {
                                Aramis != Cons.Truct(jardin, duel),
                                Athos != Cons.Truct(jardin, duel),
                                Pathos != Cons.Truct(jardin, duel)
                            }
                        },

                        new Conj()
                        {
                            Aramis_A != musket,
                            Aramis_P != hotel
                        },

                        new Disj()
                        {
                            Pathos_A != musket,
                            Constance_P != jardin
                        },

                        new Conj()
                        {
                            new Disj()
                            {
                                Pathos_P != estate,
                                new Disj()
                                {
                                    Aramis == Cons.Truct(estate, duel),
                                    Athos == Cons.Truct(estate, duel),
                                    Pathos == Cons.Truct(estate, duel)
                                }
                            },
                            new Disj()
                            {
                                new Conj()
                                {
                                    Aramis != Cons.Truct(estate, duel),
                                    Athos != Cons.Truct(estate, duel),
                                    Pathos != Cons.Truct(estate, duel)
                                },
                                Pathos_P == estate
                            }
                        },

                        Constance_P != hotel
                    })));
        }


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

            /*
            Goal g = new CallFresh((x, y) => new Conj()
            {
                new Disj()
                {
                    x == 5,
                    x == 6
                },
                y == x,
                y != 5
            });
            */

            Goal g = Puzzle();


            Console.WriteLine("Sphinx of black quartz! Judge my vow:");
            Console.WriteLine();
            //Console.WriteLine(IO.Graft(g.ToTree()));

            int answers = 0;

            using (var iter = g.Pursue().GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("     Answer #" + (answers + 1).ToString());
                    Console.WriteLine("--------------------------------------------------\n");
                    Console.WriteLine(iter.Current.ToString(0));
                    Console.WriteLine("--------------------------------------------------\n");
                    ++answers;
                }
            }

            Console.WriteLine("--------------------------------------------------\n");
            Console.WriteLine(IO.Graft(g.ToTree()));

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




