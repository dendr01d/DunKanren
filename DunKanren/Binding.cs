using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Immutable;

namespace DunKanren
{
    public abstract class Binding
    {
        private Binding() { }

        public abstract Binding ToNew();

        public abstract class Bound : Binding
        {
            public abstract ADT.Term GetValue();

            public sealed class Value<T> : Bound
                where T : ADT.Term
            {
                private T _assignment;
                public Value(T assignment)
                {
                    _assignment = assignment;
                }

                public override T GetValue() => _assignment;

                public override Binding ToNew()
                {
                    return new Value<T>(_assignment);
                }
            }
        }

        public abstract class Free : Binding
        {
            protected ImmutableHashSet<Predicate<ADT.Term>> _restrictions;
            
            private Free()
            {
                _restrictions = ImmutableHashSet.Create<Predicate<ADT.Term>>();
            }

            public bool CanBind<T>(T value)
                where T : ADT.Term
            {
                return _restrictions.All(x => x(value));
            }

            public Bound.Value<T> Bind<T>(T value)
                where T : ADT.Term
            {
                if (CanBind(value))
                {
                    return new Bound.Value<T>(value);
                }
                throw new Exception($"Value {value} of type {typeof(T)} cannot be bound.");
            }

            public abstract Free AddRestriction(Predicate<ADT.Term> rule);

            public sealed class Typed<T> : Free
                where T : ADT.Term
            {
                public Typed(params Predicate<ADT.Term>[] rules)
                {
                    _restrictions = rules.ToImmutableHashSet();
                }
                public override Typed<T> AddRestriction(Predicate<ADT.Term> rule)
                {
                    return new Typed<T>(_restrictions.Append(rule).ToArray());
                }
                public override Typed<T> ToNew() => new Typed<T>(_restrictions.ToArray());
            }
            public sealed class Untyped : Free
            {
                public Untyped(params Predicate<ADT.Term>[] rules)
                {
                    _restrictions = rules.ToImmutableHashSet();
                }
                public override Untyped AddRestriction(Predicate<ADT.Term> rule)
                {
                    return new Untyped(_restrictions.Append(rule).ToArray());
                }
                public Typed<T> AddType<T>()
                    where T : ADT.Term
                {
                    return new Typed<T>(_restrictions.ToArray());
                }
                public override Untyped ToNew() => new Untyped(_restrictions.ToArray());
            }
        }
    }
}
