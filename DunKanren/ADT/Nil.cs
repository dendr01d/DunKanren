using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.ADT
{
    public abstract partial class Term
    {
        public sealed partial class Nil
        {
            public static Nil Value = new();

            public override string ToString()
            {
                return "ⁿ";
            }
        }
    }
}
