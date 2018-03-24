using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Util
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string @this)
        {
            return @this == null || @this == String.Empty;
        }

        public static string ToForwardSlashes(this string @this)
        {
            // Super dumb but effective
            return @this.Replace(@"\", "/");
        }

    }
}
