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

#else
        public static void Message(string message)
        {
        }
#endif
    }
}
