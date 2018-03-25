using JarVerify.Container;
using JarVerify.Exceptions;
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

namespace JarVerify.Cryptography
{
    /// <summary>
    /// Enforce the validity of the signatures detected in a JAR
    /// </summary>
    public class SignatureVerifier
    {
        private readonly IManifestLoader _loader;
        
        public SignatureVerifier()
        {
            _loader = new ManifestLoader();
        }

        /// <summary>
        /// Verify all signatures in the JAR
        /// </summary>
        /// <param name="jar">JAR to verify</param>
        /// <param name="centralManifest">the main MANIFEST.MF</param>
        /// <param name="signatures">the set of signatures to verify</param>
        /// <param name="certificates">the set of permitted certificates we verify against</param>
        /// <returns>true if all signatures verify as valid - otherwise false</returns>
        public bool Verify(
            IJar jar,
            ManifestData centralManifest,
            List<Signature> signatures,
            IVerificationCertificates certificates)
        {
            if (jar == null)
            {
                throw new ArgumentNullException(nameof(jar));
            }

            if (centralManifest == null)
            {
                throw new ArgumentNullException(nameof(centralManifest));
            }

            if (certificates == null)
            {
                throw new ArgumentNullException(nameof(certificates));
            }

            if (!signatures.Any())
            {
                return false;
            }
            
            foreach (Signature sig in signatures)
            {
                ManifestData signFile = _loader.Load(jar, sig.ManifestPath);

                Log.Message($"Signature {sig.BaseName} @ {sig.ManifestPath} with block {sig.Block.Path} type {sig.Block.Type}");

                // Sign file hash mismatch
                if (!VerifyManifestHashes(centralManifest, signFile))
                {
                    return false;
                }
                
                // Ensure we actually have a certificate to verify against
                if (!certificates.Contains(sig.BaseName))
                {
                    throw new MissingCertificateException($"Signature with base name {sig.BaseName} must have a matching certificate " +
                        $"supplied in order to verify");
                }
            }

            return signatures.All(s => VerifyPKCS7(jar, s, certificates.Get(s.BaseName)));
        }

        /// <summary>
        /// Verify each of the hashes in the sign file against the data in the main MANIFEST.MF
        /// </summary>
        /// <param name="centralManifest">MANIFEST.MF data</param>
        /// <param name="signFile">the .SF data</param>
        /// <returns>whether the sign file entries verify OK</returns>
        private bool VerifyManifestHashes(ManifestData centralManifest, ManifestData signFile)
        {
            Log.Message($"Signing file contains {signFile.Entries.Count} entries");

            Log.Message($"Expecting main manifest digest of {signFile.ManifestDigest}");

            using (var h = new Hasher())
            {
                // The digest of the manifest in the JAR must match the digest of the manifest 
                // stated in the sign file 
                if (centralManifest.ManifestDigest != signFile.ManifestDigest)
                {
                    Log.Message($"Main manifest has unexpected digest {centralManifest.ManifestDigest}");

                    return false;
                }

                int centralEntry = 0;

                // Take each entry from our sign file
                foreach (ManifestEntry signed in signFile.Entries)
                {
                    // And match it up against the equiavalent entry in the main manifest
                    //
                    // TODO: Assuming manifest and SF are always the same order + length which should be true
                    ManifestEntry central = centralManifest.Entries[centralEntry];

                    centralEntry++;

                    Log.Message($"Signed digest check {signed.Path} ({signed.Digest})");

                    // The hash of each of the three lines in Manifest.MF (Name + Digest + newlines) must match up
                    // with the digest in the sign file
                    //
                    if (h.SHA256(central.Original).ToBase64() != signed.Digest)
                    {
                        Log.Message($"{signed.Path} does not match {central.Path} in main manifest ({signed.Digest} != {central.Digest})");

                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Verify a PKCS7 signature block
        /// </summary>
        /// <param name="jar">JAR from which to read and verify the signature</param>
        /// <param name="sig">the signature being verified</param>
        /// <param name="certificate">the raw certificate bytes against which to verify (i.e. public key)</param>
        /// <returns>whether the PKCS signature is valid</returns>
        private bool VerifyPKCS7(IJar jar, Signature sig, byte[] certificate)
        {
            try
            {
                // Detached content to verify - in this case, the .SF file 
                // (against which the signature block validates its hash)
                CmsProcessableByteArray detachedContent;

                // We cannot easily reuse a reader against the SF file
                // So instead, copy to memory in entirety and build from byte array
                using (Stream sigFile = jar.Open(sig.ManifestPath))
                using (MemoryStream sigFileMemory = new MemoryStream())
                {
                    sigFile.CopyTo(sigFileMemory);

                    detachedContent = new CmsProcessableByteArray(sigFileMemory.ToArray());
                }

               // Open the signature block (e.g. .RSA or .DSA)
                using (Stream block = jar.Open(sig.Block.Path))
                {
                    X509CertificateParser certParser = new X509CertificateParser();

                    // Read the caller's certificate (assumed to have a matching public key)
                    X509Certificate cert = certParser.ReadCertificate(certificate);

                    CmsSignedData signedData = new CmsSignedData(detachedContent, block);

                    SignerInformationStore signers = signedData.GetSignerInfos();

                    int verified = 0;

                    foreach (SignerInformation signer in signers.GetSigners())
                    {
                        Log.Message($"Verifying against {cert.SubjectDN.ToString()}");

                        if (signer.Verify(cert))
                        {
                            verified++;

                            Log.Message($"Signature valid for {cert.SubjectDN.ToString()}");
                        }
                        else
                        {
                            Log.Message($"Signature INVALID for {cert.SubjectDN.ToString()}");
                        }
                    }

                    // Every signer must verify OK
                    return verified == signers.GetSigners().Count;
                }
            }
            catch (Exception ex)
            {
                // Cert verification can trigger a number of different possible errors
                // (Ranging from cert bytes invalid -> key type mismatch)

                Log.Error(ex, "Failed to verify certifiate: assuming invalid");

                return false;
            }
        }
    }
}
