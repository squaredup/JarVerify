using JarVerify.Cryptography;
using JarVerify.Exceptions;
using JarVerify.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Container
{
    public static class JarExtensions
    {
        /// <summary>
        /// Produce a SHA256 of a file in the JAR. If the file does not exist, an exception is thrown.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="hasher">hasher implementation</param>
        /// <param name="path">filename to generate a hash of</param>
        /// <returns>the byte data of the SHA-256 hash</returns>
        public static byte[] SHA256(this IJar @this, Hasher hasher, string path)
        {
            if (hasher == null)
            {
                throw new ArgumentNullException(nameof(hasher));
            }

            if (path.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!@this.Contains(path))
            {
                throw new JarException($"File to hash {path} does not exist in JAR");
            }

            using (Stream file = @this.Open(path))
            {
                return hasher.SHA256(file);
            }
        }

        /// <summary>
        /// Get only the files in the JAR that have no connection to the signing process 
        /// </summary>
        /// <param name="this"></param>
        /// <returns>set of files which are not metadata/signing related</returns>
        public static IEnumerable<string> NonSignatureFiles(this IJar @this)
        {
            string[] manifestExtensions = new string[]
            {
                ".RSA", 
                ".DSA", 
                ".MF", 
                ".SF"
            };

            return @this.Files()
                .Where(f =>
                {
                    if (f.ToForwardSlashes().StartsWith(@"META-INF/"))
                    {
                        // Is it a manifest or signature?
                        if (manifestExtensions.Any(ext => 
                            ext.Equals(Path.GetExtension(f), StringComparison.InvariantCultureIgnoreCase)))
                        {
                            return false;
                        }

                        return true;
                    }
                    else
                    {
                        // Not metadata
                        return true;
                    }
                });
        }
    }
}
