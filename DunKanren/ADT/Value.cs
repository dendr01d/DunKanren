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
            public abstract partial class TypedValue<T> : Term
            {
                private T _data;

                private TypedValue(T data)
                {
                    _data = data;
                }

                public override string ToString()
                {
                    return _data?.ToString() ?? "UNK VALUE";
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
