using JarVerify.Container;
using JarVerify.Cryptography;
using JarVerify.Manifest;
using JarVerify.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify
{
    public enum SigningStatus
    {
        NotSigned,

        /// <summary>
        /// Regardless of signing state the files in the JAR do not match the manifest
        /// </summary>
        HashMismatch,


        SignedValid
    }

    public static class Verify
    {
        public static SigningStatus Jar(string filename, bool nonStandardCountCheck = true)
        {
            using (IJar jar = new Jar(filename))
            {
                // Unsigned ZIP and probably not even a JAR
                if (!jar.Contains(@"META-INF\MANIFEST.MF"))
                {
                    return SigningStatus.NotSigned;
                }
                
                IManifestLoader manifestLoader = new ManifestLoader();

                ManifestData centralManifest = manifestLoader.Load(jar, @"META-INF\MANIFEST.MF");

                if (nonStandardCountCheck)
                {
                    // Non-standard check: Ensure that no unsigned files have been ADDED
                    // to the JAR (file qty. except signature itself must match manifest entries)
                    //
                    int nonManifestFiles = jar.NonSignatureFiles().Count();

                    if (centralManifest.Entries.Count != nonManifestFiles)
                    {
                        Log.Message($"Expected {centralManifest.Entries.Count} file(s) found {nonManifestFiles}");

                        return SigningStatus.HashMismatch;
                    }
                }

                using (var h = new Hasher())
                {
                    Log.Message($"Central manifest contains {centralManifest.Entries.Count} entries");

                    foreach (ManifestEntry e in centralManifest.Entries)
                    {
                        Log.Message($"Digest check {e.Path} ({e.Digest})");

                        // Check each file matches the hash in the manifest
                        if (jar.SHA256(h, e.Path).ToBase64() != e.Digest)
                        {
                            Log.Message($"{e.Path} has an incorrect digest");

                            return SigningStatus.HashMismatch;
                        }
                    }
                }

                SignatureFinder finder = new SignatureFinder();

                List<Signature> signatures = finder.Find(jar);

                if (!signatures.Any())
                {
                    Log.Message("No signatures detected");

                    return SigningStatus.NotSigned;
                }

                Log.Message($"{signatures.Count} signature(s) detected");

                foreach (Signature sig in signatures)
                {
                    ManifestData signFile = manifestLoader.Load(jar, sig.ManifestPath);

                    Log.Message($"Signature {sig.BaseName} @ {sig.ManifestPath} with block {sig.Block.Path} type {sig.Block.Type}");

                    Log.Message($"Signing file contains {signFile.Entries.Count} entries");

                    Log.Message($"Expecting main manifest digest of {signFile.ManifestDigest}");

                    using (var h = new Hasher())
                    {
                        if (centralManifest.ManifestDigest != signFile.ManifestDigest)
                        {
                            Log.Message($"Main manifest has unexpected digest {centralManifest.ManifestDigest}");

                            return SigningStatus.HashMismatch;
                        }

                        int centralEntry = 0;

                        // Take each entry from our sign file
                        foreach (ManifestEntry signed in signFile.Entries)
                        {
                            // And match it up against the equiavalent entry in the main manifest
                            ManifestEntry central = centralManifest.Entries[centralEntry];

                            centralEntry++;
                            
                            Log.Message($"Signed digest check {signed.Path} ({signed.Digest})");

                            // So hash the hash of the text in MANIFEST.MF and compare against the SF hash
                            if (h.SHA256(central.Original).ToBase64() != signed.Digest)
                            {
                                Log.Message($"{signed.Path} does not match {central.Path} in main manifest ({signed.Digest} != {central.Digest})");

                                return SigningStatus.HashMismatch;
                            }
                        }
                    }

                }
            }

            return SigningStatus.SignedValid;
        }
    }
}
