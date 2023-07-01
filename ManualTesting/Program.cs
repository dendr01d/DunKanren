using DunKanren.Goals;

namespace DunKanren
{
    internal class Program
    {
        static Goal Conso(Term car, Term cdr, Term cons)
        {
            return new Conj()
            {
                LCons.Truct(car, cdr) == cons,
                //(cons is LCons ? new Top() : new Bottom()),
                //((cons as LCons)?.Car ?? Term.NIL) == car,
                //((cons as LCons)?.Cdr ?? Term.NIL) == cdr,
            };
        }

        static Goal Appendo(Term a, Term b, Term c)
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
                        LCons.Truct(first, aRest) == a,
                        LCons.Truct(first, cRest) == c,
                        Appendo(aRest, b, cRest)
                    })
                }
            };
        }

        static Goal Membero(Term a, Term coll)
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

        static Goal NotMembero(Term a, Term coll)
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
                            Membero(Aramis_P, LCons.TructList(hotel, jardin, estate)),
                            Membero(Athos_P, LCons.TructList(hotel, jardin, estate)),
                            Membero(Pathos_P, LCons.TructList(hotel, jardin, estate)),
                            Membero(Constance_P, LCons.TructList(hotel, jardin, estate))
                        },

                        new Conj()
                        {
                            Membero(Aramis_A, LCons.TructList(musket, duel, rendM)),
                            Membero(Athos_A, LCons.TructList(musket, duel, rendM)),
                            Membero(Pathos_A, LCons.TructList(musket, duel, rendM)),
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

        //static Goal PuzzleNew()
        //{
        //    return new CallFresh((Aramis, Athos, Pathos, Constance) =>
        //        new CallFresh((Aramis_Doing, Athos_Doing, Pathos_Doing, Constance_Doing, musket, rendezvous, duel) =>
        //            new CallFresh((Aramis_Location, Athos_Location, Pathos_Location, Constance_Location, hotel, jardin, estate) => new Conj()
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

        //                Bijecto(LCons.TructList(Aramis_Doing, Athos_Doing, Pathos_Doing), LCons.TructList(musket, rendezvous, duel)),
        //                Bijecto(LCons.TructList(Aramis_Location, Athos_Location, Pathos_Location), LCons.TructList(hotel, jardin, estate)),

        //                new Impl
        //                (
        //                    new Disj()
        //                    {
        //                        new Conj(Aramis_Doing == musket, Aramis_Location == hotel),
        //                        new Conj(Athos_Doing == musket, Athos_Location == hotel ),
        //                        new Conj(Pathos_Doing == musket, Pathos_Location == hotel)
        //                    },
        //                    new Disj()
        //                    {
        //                        Aramis_Location == jardin,
        //                        Athos_Location == jardin
        //                    }
        //                ),

        //                Pathos_Doing != duel,

        //                new DNF()
        //                {
        //                    { Aramis_Doing == duel, Aramis_Location == jardin },
        //                    { Athos_Doing == duel, Athos_Location == jardin },
        //                    { Pathos_Doing == duel, Pathos_Location == jardin }
        //                }.Negate(),

        //                Aramis_Doing != musket,
        //                Aramis_Location != hotel,

        //                new Impl(Pathos_Doing == musket, Constance_Location != jardin),
        //                new BImp(
        //                    new DNF()
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


        //https://leanprover.github.io/logic_and_proof/propositional_logic.html
        public static Goal MurderPuzzle()
        {

            return
                new CallFresh((Alice, Husband, Brother, Son, Daughter) =>
                new CallFresh((BarMan, BarWoman, Killer, Victim, Loner) =>
                new Conj()
            {
                    BarMan == ValueFactory.Sym("Man at the bar"),
                    BarWoman == ValueFactory.Sym("Woman at the bar"),
                    Killer == ValueFactory.Sym("Killer at the beach"),
                    Victim == ValueFactory.Sym("Victim at the beach"),
                    Loner == ValueFactory.Sym("Bystander elsewhere"),

                    Membero(Alice, LCons.TructList(BarMan, BarWoman, Killer, Victim, Loner)),
                    Membero(Husband, LCons.TructList(BarMan, BarWoman, Killer, Victim, Loner)),
                    Membero(Brother, LCons.TructList(BarMan, BarWoman, Killer, Victim, Loner)),
                    Membero(Son, LCons.TructList(BarMan, BarWoman, Killer, Victim, Loner)),
                    Membero(Daughter, LCons.TructList(BarMan, BarWoman, Killer, Victim, Loner)),

                    new Conj()
                    {
                        Alice != Husband, Alice != Brother, Alice != Son, Alice != Daughter,
                        Husband != Brother, Husband != Son, Husband != Daughter,
                        Brother != Son, Brother != Daughter,
                        Son != Daughter,
                    },

                    //setup complete?

                    Membero(BarMan, LCons.TructList(Husband, Brother, Son)),
                    Membero(BarWoman, LCons.TructList(Alice, Daughter)),

                    new XOR()
                    {
                        Loner == Son,
                        Loner == Daughter
                    },

                    new BImp(Alice == BarWoman, new Disj(Husband == Victim, Husband == Killer)),
                    new BImp(Husband == BarMan, new Disj(Alice == Victim, Alice == Killer)),

                    Husband != Victim, //the victim's twin must be amongst the 5 people, but the husband can't have one
                    new Disj(Son != Killer, Daughter != Victim),
                    new Disj(Son != Victim, Daughter != Killer),
                    new Disj(Alice != Killer, Brother != Victim),
                    new Disj(Alice != Victim, Brother != Killer),

                    //new Disj()
                    //{
                    //    new Impl(Alice == Victim, Brother != Killer),
                    //    new Impl(Brother == Victim, Alice != Killer),
                    //    new Impl(Son == Victim, Daughter != Killer),
                    //    new Impl(Daughter == Victim, Son != Killer),
                    //},

                    new Impl(Son == Victim, new Conj(Alice != Killer, Husband != Killer)),
                    new Impl(Daughter == Victim, new Conj(Alice != Killer, Husband != Killer)),
                    //new Impl(Alice == Victim, new Disj(Son == Killer, Daughter == Killer)),
                    //new Impl(Husband == Victim, new Disj(Son == Killer, Daughter == Killer)),

                    //new XOR(Alice == BarMan, Alice == BarWoman, Alice == Killer, Alice == Victim, Alice == Loner),
                    //new XOR(Husband == BarMan, Husband == BarWoman, Husband == Killer, Husband == Victim, Husband == Loner),
                    //new XOR(Brother == BarMan, Brother == BarWoman, Brother == Killer, Brother == Victim, Brother == Loner),
                    //new XOR(Son == BarMan, Son == BarWoman, Son == Killer, Son == Victim, Son == Loner),
                    //new XOR(Daughter == BarMan, Daughter == BarWoman, Daughter == Killer, Daughter == Victim, Daughter == Loner),


            }));
        }

        static void Main()
        {
            //Goal g = MurderPuzzle();

            //Goal g = new CallFresh((x, y) => new Conj(x == 5, y == 6));

            Goal g = new CallFresh((x, y) => Appendo(x, y, "abc"));

            //Goal g = new CallFresh((x, y, z, w) => new Conj() {
            //    Appendo(x, y, z),
            //    Appendo(z, w, "abcd")
            //});

            //Goal g = new CallFresh(x => Bijecto("abc", x));

            //Goal g = new CallFresh(x => Sharedo("abc", x));

            //Goal g = new CallFresh(x => Membero(x, LCons.TructList('a', 'b', 'c')));

            //Goal g = new CallFresh(x => NotMembero('b', LCons.TructList('a', x, 'c')));

            //Goal g = new CallFresh(x => Removeo('a', "abcd", x));

            //Goal g = new CallFresh(x => Reverseo(x, "abcd"));

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
                    IO.Prompt(true);
                    Console.WriteLine();
                    Console.WriteLine(IO.Graft(g.ToTree()));
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
            Console.WriteLine();
            IO.Prompt(true);

            Environment.Exit(0);
        }
    }
}




