using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JarVerify.Container;
using JarVerify.Cryptography;
using JarVerify.Exceptions;
using JarVerify.Util;

namespace JarVerify.Manifest
{
    public class ManifestLoader : IManifestLoader
    {
        /// <summary>
        /// Load all manifest entries from a given manifest in a JAR
        /// </summary>
        /// <param name="source">source JAR</param>
        /// <param name="manifest">manifest to load</param>
        /// <returns>manifest data</returns>
        public ManifestData Load(IJar source, string path)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (path.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(path));
            }

            // If this file does not exist, obviously we cannot load it
            if (!source.Contains(path))
            {
                throw new ManifestException($"Manifest {path} does not exist");
            }

            ManifestData manifest = new ManifestData
            {
                ManifestDigest = String.Empty,
                Entries = new List<ManifestEntry>()
            };

            // The manifest digest is supposed to refer to the digest that THIS manifest
            // expects the main manifest to have. Until set otherwise, assume this digest
            // is = to our own hash (which means loading the main manifest populates this 
            // value with the main manifest hash for future comparison)
            using (Hasher h = new Hasher())
            {
                manifest.ManifestDigest = source.SHA256(h, path).ToBase64();
            }

            try
            {
                using (StreamReader reader = new StreamReader(source.Open(path)))
                {
                    string[] lines = Unwrap70(reader.ReadToEnd().Split(
                        new char[]
                        {
                        (char)10,
                        (char)13
                        },
                        StringSplitOptions.RemoveEmptyEntries));
                    
                    Populate(manifest, lines);
                }
            }
            catch(Exception ex)
            {
                throw new ManifestException($"Failed to open or parse manifest {path}", ex);
            }

            return manifest;
        }
       
        /// <summary>
        /// Populate the entries in a manfiest from the given text lines
        /// </summary>
        /// <param name="manifest">manifest to populate</param>
        /// <param name="lines">individual manifest lines</param>
        private void Populate(ManifestData manifest, string[] lines)
        {
            for (int ptr = 0; ptr < lines.Length; ptr++)
            {
                // Split each line into  NAME: VALUE
                string[] parts = lines[ptr].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    // Is it a filename? 
                    if (parts[0].Equals("Name", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // There must be a digest so split the next line in half too
                        string[] digestParts = lines[ptr + 1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                        manifest.Entries.Add(new ManifestEntry
                        {
                            Path = parts[1].TrimStart(),
                            Digest = digestParts[1].TrimStart(),

                            // Preserve spacing structure for SF hashing comparison
                            //
                            // The newlines are important and this is the only document that really tells you:
                            // https://docs.oracle.com/javase/7/docs/technotes/tools/windows/jarsigner.html
                            // "hash of the **three** lines in the manifest file for the source file."
                            // 
                            Original =
                                Wrap70(lines[ptr]) + Environment.NewLine +
                                Wrap70(lines[ptr + 1]) + Environment.NewLine + Environment.NewLine
                        });

                        // Skip the line after us because have already read it
                        ptr++;
                        continue;
                    }

                    // Is it the manifest hash?
                    if (parts[0].EndsWith("Digest-Manifest", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Second half MUST be the hash (and MANIFEST.MF cannot wrap)
                        manifest.ManifestDigest = parts[1].TrimStart();
                    }
                }
            }
        }

        /// <summary>
        /// Unwrap lines which have been split across the 70 character boundary, for ease of parsing
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private string[] Unwrap70(string[] lines)
        {
            List<string> rebuilt = new List<string>();

            foreach (string line in lines)
            {
                // Starting with a space indicates a wrap from the previous line
                if (char.IsWhiteSpace(line[0]))
                {
                    // Append this line onto the previous line, removing the first character
                    // Intentionally don't trim here as the line could have two spaces, e.g. 
                    // if the string was split on a space in the filename.
                    rebuilt[rebuilt.Count - 1] += line.Substring(1);
                }
                else
                {
                    rebuilt.Add(line);
                }
            }

            return rebuilt.ToArray();
        }

        /// <summary>
        /// Re-wrap a long line into the 70/72-limited lines as per original text (for hashing)
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string Wrap70(string line)
        {
            if (line.Length > 70)
            {
                // TODO: Should really be recursive 

                return line.Substring(0, 70) + Environment.NewLine + " " + line.Substring(70);
            }
            else
            {
                return line;
            }
        }
               
    }
}
