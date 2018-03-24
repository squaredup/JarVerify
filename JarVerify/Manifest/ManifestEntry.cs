using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Manifest
{
    public class ManifestEntry
    {
        /// <summary>
        /// File of the associated file, if any
        /// </summary>
        public string Path
        {
            get;
            set;
        }

        /// <summary>
        /// Digest as base64 (SHA256 specifically)
        /// </summary>
        public string Digest
        {
            get;
            set;
        }

        /// <summary>
        /// Original text from which a hash can be calculated (e.g. for .SF comparison)
        /// </summary>
        public string Original
        {
            get;
            set;
        }
    }
}
