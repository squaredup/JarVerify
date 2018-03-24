using JarVerify.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Manifest
{
    public class ManifestData
    {
        /// <summary>
        /// The digest stipulated against the main manifest (optional)
        /// </summary>
        public string ManifestDigest
        {
            get;
            set;
        }

        public List<ManifestEntry> Entries
        {
            get;
            set;
        }
    }

    public interface IManifestLoader
    {
        /// <summary>
        /// Load all manifest entries from a given manifest in a JAR
        /// </summary>
        /// <param name="source">source JAR</param>
        /// <param name="manifest">manifest to load</param>
        /// <returns>manifest data</returns>
        ManifestData Load(IJar source, string manifest);
    }
}
