using JarVerify.Manifest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Container
{
    public class SignatureFinder
    {        
        public List<Signature> Find(IJar jar)
        {
            // Signature from base name -> data
            Dictionary<string, Signature> found = new Dictionary<string, Signature>(StringComparer.InvariantCultureIgnoreCase);

            foreach(string candidate in jar.Files())
            {
                string[] pathParts = candidate.Split('/');

                // Must be in META-INF
                if (pathParts.Length == 2 && pathParts[0] == "META-INF")
                {
                    string filename = pathParts[1];

                    if (filename.Equals("MANIFEST.MF", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // We don't care about the non-signature manifest
                        continue;
                    }

                    // Base name being the actual overall signature name
                    string baseName = Path.GetFileNameWithoutExtension(filename);

                    // If there is no signature yet for this name, add one 
                    if (!found.ContainsKey(baseName))
                    {
                        found.Add(baseName, new Signature
                        {
                            BaseName = baseName
                        }); 
                    }

                    string extension = Path.GetExtension(filename);

                    Signature sig = found[baseName];

                    switch(extension.Substring(1).ToLowerInvariant())
                    {
                        case "rsa":
                            sig.Block = new SignatureBlockFile
                            {
                                Type = SignatureBlockType.RSA, 
                                Path = candidate
                            };
                            break;

                        case "dsa":
                            sig.Block = new SignatureBlockFile
                            {
                                Type = SignatureBlockType.DSA,
                                Path = candidate
                            };
                            break;

                        case "sf":
                            sig.ManifestPath = candidate;
                            break;
                    }
                }
            }

            return found.Values.ToList();
        }
    }
}
