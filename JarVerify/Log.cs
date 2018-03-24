using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify
{
    public static class Log
    {
#if DEBUG

        public static void Message(string message)
        {
            Debugger.Log(1, String.Empty, message + Environment.NewLine);
            Console.WriteLine(message);
        }

        public static void Error(Exception ex, string message)
        {
            Debugger.Log(1, String.Empty, message + Environment.NewLine);
            Debugger.Log(1, String.Empty, ex + Environment.NewLine);

            Console.WriteLine(message);
            Console.WriteLine(ex);
        }
#else
        public static void Message(string message)
        {
        }

        public static void Error(Exception ex,string message)
        {
        }
#endif
    }
}
