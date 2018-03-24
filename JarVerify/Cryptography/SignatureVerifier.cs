using JarVerify.Container;
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
    public class SignatureVerifier
    {
        private readonly IManifestLoader _loader;
        
        public SignatureVerifier()
        {
            _loader = new ManifestLoader();
        }

        public bool Verify(
            IJar jar,
            ManifestData centralManifest,
            List<Signature> signatures,
            IVerificationCertificates certificates)
        {
            foreach (Signature sig in signatures)
            {
                ManifestData signFile = _loader.Load(jar, sig.ManifestPath);

                Log.Message($"Signature {sig.BaseName} @ {sig.ManifestPath} with block {sig.Block.Path} type {sig.Block.Type}");

                Log.Message($"Signing file contains {signFile.Entries.Count} entries");

                Log.Message($"Expecting main manifest digest of {signFile.ManifestDigest}");

                if (!certificates.Contains(sig.BaseName))
                {
                    // throw
                }

                using (var h = new Hasher())
                {
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

                        // So hash the hash of the text in MANIFEST.MF and compare against the SF hash
                        if (h.SHA256(central.Original).ToBase64() != signed.Digest)
                        {
                            Log.Message($"{signed.Path} does not match {central.Path} in main manifest ({signed.Digest} != {central.Digest})");

                            return false;
                        }
                    }
                }
            }

            return signatures.All(s => VerifyPKCS7(jar, s, certificates.Get(s.BaseName)));
        }

        private bool VerifyPKCS7(IJar jar, Signature sig, byte[] certificate)
        {
            try
            {
                using (Stream sigFile = jar.Open(sig.ManifestPath))
                using (Stream block = jar.Open(sig.Block.Path))
                using (MemoryStream test = new MemoryStream())
                {
                    X509CertificateParser certParser = new X509CertificateParser();
                    X509Certificate cert = certParser.ReadCertificate(certificate);

                    sigFile.CopyTo(test);
                    CmsProcessableByteArray input = new CmsProcessableByteArray(test.ToArray());

                    CmsSignedData d = new CmsSignedData(input, block);

                    var signers = d.GetSignerInfos();

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

                    return verified == signers.GetSigners().Count;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to verify certifiate: assuming invalid");

                return false;
            }
        }
    }
}
