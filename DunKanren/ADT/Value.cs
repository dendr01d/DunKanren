using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.ADT
{
    public abstract partial class Term
    {
        public abstract partial class Value : Term
        {
            public abstract bool Equals(Value? other);

            public abstract partial class TypedValue<T> : Term
                where T : IContextuallyEquatable<T>
            {
                public T Data { get; private set; }

                private TypedValue(T data)
                {
                    Data = data;
                }

                public bool Equals(Value? other)
                {
                    return other is TypedValue<T> typedOther
                        && Equals(typedOther.Data, Data);
                }

                public override string ToString()
                {
                    return Data?.ToString() ?? "NULL-VALUE";
                }

                public sealed class KChar : TypedValue<char>
                {
                    public KChar(char data) : base(data) { }
                }

                public sealed class KInt : TypedValue<int>
                {
                    public KInt(int data) : base(data) { }
                }

                public sealed class KBool : TypedValue<bool>
                {
                    private KBool(bool data) : base(data) { }
                    public static Term True => new KBool(true);
                    public static Term False => new KBool(false);
                }
            }
        }
    }
}
