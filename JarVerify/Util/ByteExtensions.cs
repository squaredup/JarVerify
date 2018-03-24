using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Util
{
    public static class ByteExtensions
    {
        public static string ToBase64(this byte[] @this)
        {
            return Convert.ToBase64String(@this);
        }
    }
}
