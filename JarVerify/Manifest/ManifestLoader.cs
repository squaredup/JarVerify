using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JarVerify.Container;
using JarVerify.Cryptography;
using JarVerify.Util;

namespace JarVerify.Manifest
{
    public class ManifestLoader : IManifestLoader
    {
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

            using (StreamReader reader = new StreamReader(source.Open(path)))
            {
                string[] lines = reader.ReadToEnd().Split(
                    new char[] { (char)10, (char)13 }, StringSplitOptions.RemoveEmptyEntries);

                // Manifest files wrap at 70 (or 72 w/ CR+LR) characters
                // 
                // Rebuild what we read to remove these line breaks
                List<string> rebuilt = new List<string>();

                foreach (string line in lines)
                {
                    if (line.TrimStart() != line)
                    {
                        // Append this line onto the previous line
                        rebuilt[rebuilt.Count - 1] += line.TrimStart();
                    }
                    else
                    {
                        rebuilt.Add(line);
                    }
                }

                lines = rebuilt.ToArray();

                for (int ptr = 0; ptr < lines.Length; ptr++)
                {
                    string line = lines[ptr];

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
                                Original =
                                    Rewrap(lines[ptr]) + Environment.NewLine +
                                    Rewrap(lines[ptr + 1]) + Environment.NewLine + Environment.NewLine
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

            return manifest;
        }

        /// <summary>
        /// Re-wrap a long line into the 70/72-limited lines as per original text (for hashing)
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string Rewrap(string line)
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
