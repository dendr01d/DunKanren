using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public static class IO
    {
        private static bool PromptUser = false;


        public static void DisablePrompting()
        {
            PromptUser = false;
        }

        public static void Prompt(bool manualOverride = false)
        {
            if (PromptUser || manualOverride)
            {
                Console.WriteLine("[<─┘]");
                Console.ReadKey(true);
                Console.CursorTop--;
                Console.Write("     ");
                Console.CursorLeft = 0;
            }
        }

        [Conditional("DEBUG")]
        public static void Debug_Prompt()
        {
            IO.Prompt();
        }

        [Conditional("DEBUG")]
        public static void Debug_Print(string s)
        {
            Console.WriteLine(s);
            Prompt();
        }

        [Conditional("DEBUG")]
        public static void Debug_Print(string s, bool conditional)
        {
            if (conditional) Debug_Print(s);
        }

        [Conditional("DEBUG")]
        public static void Debug_Print(IEnumerable<string> ss)
        {
            foreach (string s in ss) Console.WriteLine(s);
            Prompt();
        }

        [Conditional("DEBUG")]
        public static void Debug_Print(IEnumerable<string> ss, bool conditional)
        {
            if (conditional) Debug_Print(ss);
        }


        public const string HEADER = "┬";
        public const string ALONER = "─";
        public const string BRANCH = "├─";
        public const string JUMPER = "│ ";
        public const string SPACER = "  ";
        public const string LEAVES = "└─";


        public static IEnumerable<string> PrependBranches(IEnumerable<string> branches, string first, string rest)
        {
            yield return first + branches.First();

            foreach(string branch in branches.Skip(1))
            {
                yield return rest + branch;
            }
        }

        public static string Graft(IEnumerable<string> branches)
        {
            var sb = new StringBuilder();
            
            foreach(string branch in branches.Where(x => !String.IsNullOrWhiteSpace(x)))
            {
                sb.AppendLine(branch);
            }

            return sb.ToString();
        }


    }

    public interface IPrintable
    {
        string ToString();
        string ToVerboseString();
        IEnumerable<string> ToTree();
        IEnumerable<string> ToTree(string prefix, bool first, bool last);
    }

    /*
    public abstract class TreeNode<T> where T : IPrintable
    {
        protected T Element;

        public TreeNode(T element)
        {
            this.Element = element;
        }

        public IEnumerable<string> GetTree()
        {

        }

        private IEnumerable<string> TreeHelper()
        {

        }
    }

    public class Branch<T1, T2> : TreeNode<T1>, IEnumerable<T2>
        where T1 : IPrintable
        where T2 : TreeNode<IPrintable>
    {

        protected List<T2> Children;

        public Branch(T1 element, params T2[] children) : base(element)
        {
            this.Children = children.ToList();
        }
    }

    public class Leaf<T> : TreeNode<T>
    {

    }
    */
}
