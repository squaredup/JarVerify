using JarVerify.Manifest;
using JarVerify.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Container
{
    public class SignatureFinder : ISignatureFinder
    {        
        public List<Signature> Find(IJar jar)
        {
            if (jar == null)
            {
                throw new ArgumentNullException(nameof(jar));
            }

            // Signature from base name -> data
            Dictionary<string, Signature> found = new Dictionary<string, Signature>(StringComparer.InvariantCultureIgnoreCase);

            foreach(string candidate in jar.Files())
            {
                string[] pathParts = candidate.Split('/');

                // Must be in META-INF
                if (pathParts.Length == 2 && pathParts[0] == "META-INF")
                {
                    string filenameOnly = pathParts[1];

                    if (filenameOnly.Equals("MANIFEST.MF", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // We don't care about the non-signature manifest
                        continue;
                    }

                    // Base name being the actual overall signature name
                    string baseName = Path.GetFileNameWithoutExtension(filenameOnly);

                    Populate(found.AddOrGet(baseName, () => new Signature
                    {
                        BaseName = baseName
                    }), 
                    candidate, filenameOnly);
                }
            }

            return found.Values
                .Where(s => !string.IsNullOrEmpty(s.ManifestPath) || s.Block != null)
                .ToList();
        }

        private void Populate(Signature sig, string path, string filenameOnly)
        {            
            string extension = Path.GetExtension(filenameOnly);
            
            // Determine the signature type OR store the path to the .SF
            switch (extension.Substring(1).ToLowerInvariant())
            {
                case "rsa":
                    sig.Block = new SignatureBlockFile
                    {
                        Type = SignatureBlockType.RSA,
                        Path = path
                    };
                    break;

                case "dsa":
                    sig.Block = new SignatureBlockFile
                    {
                        Type = SignatureBlockType.DSA,
                        Path = path
                    };
                    break;

                case "sf":
                    sig.ManifestPath = path;
                    break;
            }
        }
    }
}
