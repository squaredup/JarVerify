using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Util
{
    public static class DictionaryExtensions
    {
        public static V AddOrGet<K, V>(this Dictionary<K,V> @this, K key, Func<V> factory)
        {
            if (@this.ContainsKey(key))
            {
                return @this[key];
            }
            else
            {
                @this.Add(key, factory());

                return @this[key];
            }
        }
    }
}
