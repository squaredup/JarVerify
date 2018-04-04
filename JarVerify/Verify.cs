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
        /// <summary>
        /// Perform JAR digital signature verification against a JAR filename on disk
        /// </summary>
        /// <param name="filename">JAR filename</param>
        /// <param name="certificates">certificate to verify / accept against</param>
        /// <param name="nonStandardCountCheck">whether to perform the additional file count verification check against 
        /// MANIFEST.MF (recommended if the file is actually an arbitrary ZIP)</param>
        /// <returns>digital signature verification state of the JAR</returns>
        public static VerificationResult Jar(string filename, IVerificationCertificates certificates,
            bool nonStandardCountCheck = true)
        {
            if (filename.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (certificates == null)
            {
                throw new ArgumentNullException(nameof(certificates));
            }


            using (IJar jar = new Jar(filename))
            {
                return Jar(jar, certificates, nonStandardCountCheck);
            }
        }

        /// <summary>
        /// Perform JAR digital signature verification against a JAR filename on disk
        /// </summary>
        /// <param name="stream">JAR file stream</param>
        /// <param name="certificates">certificate to verify / accept against</param>
        /// <param name="nonStandardCountCheck">whether to perform the additional file count verification check against 
        /// MANIFEST.MF (recommended if the file is actually an arbitrary ZIP)</param>
        /// <returns>digital signature verification state of the JAR</returns>
        public static VerificationResult Jar(Stream stream, IVerificationCertificates certificates,
            bool nonStandardCountCheck = true)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (certificates == null)
            {
                throw new ArgumentNullException(nameof(certificates));
            }

            using (IJar jar = new Jar(stream))
            {
                return Jar(jar, certificates, nonStandardCountCheck);
            }
        }

        /// <summary>
        /// Perform JAR digital signature verification against a JAR filename on disk
        /// </summary>
        /// <param name="jar">JAR container. The caller is expected to dispose this type themselves - it will not be disposed
        /// by this method</param>
        /// <param name="certificates">certificate to verify / accept against</param>
        /// <param name="nonStandardCountCheck">whether to perform the additional file count verification check against 
        /// MANIFEST.MF (recommended if the file is actually an arbitrary ZIP)</param>
        /// <returns>digital signature verification state of the JAR</returns>
        public static VerificationResult Jar(IJar jar, IVerificationCertificates certificates, bool nonStandardCountCheck = true)
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
