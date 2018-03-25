using JarVerify.Container;
using JarVerify.Cryptography;
using JarVerify.Manifest;
using JarVerify.Util;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
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
        FundamentalHashMismatch,
        
        /// <summary>
        /// There are signatures and all of them are valid
        /// </summary>
        SignedValid,

        /// <summary>
        /// There are signatures and at least one is invalid
        /// </summary>
        SignedInvalid           
    }

    public class VerificationResult
    {
        public bool Valid { get; set; }
        public SigningStatus Status { get; set; }
    }

    public static class Verify
    {
        public static VerificationResult Jar(string filename, IVerificationCertificates certificates, bool nonStandardCountCheck = true)
        {
            if (filename.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (certificates == null)
            {
                throw new ArgumentNullException(nameof(certificates));
            }

            return Jar(new Jar(filename), certificates, nonStandardCountCheck);
        }

        public static VerificationResult Jar(Stream stream, IVerificationCertificates certificates, bool nonStandardCountCheck = true)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (certificates == null)
            {
                throw new ArgumentNullException(nameof(certificates));
            }

            return Jar(new Jar(stream), certificates, nonStandardCountCheck);
        }

        private static VerificationResult Jar(IJar alreadyOpen, IVerificationCertificates certificates, bool nonStandardCountCheck = true)
        {
            using (IJar jar = alreadyOpen)
            {
                // Unsigned ZIP and probably not even a JAR
                if (!jar.Contains(@"META-INF\MANIFEST.MF"))
                {
                    return new VerificationResult
                    {
                        Status = SigningStatus.NotSigned,
                        Valid = false
                    };
                }
                
                IManifestLoader manifestLoader = new ManifestLoader();

                ManifestData centralManifest = manifestLoader.Load(jar, @"META-INF\MANIFEST.MF");

                if (nonStandardCountCheck)
                {
                    // Non-standard check: Ensure that no unsigned files have been ADDED
                    // to the JAR (file qty. [except signature itself] must match manifest entries)
                    //
                    int nonManifestFiles = jar.NonSignatureFiles().Count();

                    if (centralManifest.Entries.Count != nonManifestFiles)
                    {
                        Log.Message($"Expected {centralManifest.Entries.Count} file(s) found {nonManifestFiles}");

                        return new VerificationResult
                        {
                            Status = SigningStatus.FundamentalHashMismatch,
                            Valid = false
                        };
                    }
                }

                // Verify the hashes of every file in the JAR
                //
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

                            return new VerificationResult
                            {
                                Status = SigningStatus.FundamentalHashMismatch,
                                Valid = false
                            };
                        }
                    }
                }

                // Detect signatures
                //
                //
                ISignatureFinder finder = new SignatureFinder();
                
                List<Signature> signatures = finder.Find(jar);

                if (!signatures.Any())
                {
                    Log.Message("No signatures detected");

                    return new VerificationResult
                    {
                        Status = SigningStatus.NotSigned,
                        Valid = false
                    };
                }

                Log.Message($"{signatures.Count} signature(s) detected");

                // Verify signatures
                //
                //
                SignatureVerifier ver = new SignatureVerifier();

                if (ver.Verify(jar, centralManifest, signatures, certificates))
                {
                    return new VerificationResult
                    {
                        Status = SigningStatus.SignedValid,
                        Valid = true
                    };
                }
                else
                {
                    return new VerificationResult
                    {
                        Status = SigningStatus.SignedInvalid,
                        Valid = false
                    };
                }
            }           
        }
    }
}
