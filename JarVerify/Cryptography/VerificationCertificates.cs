using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Cryptography
{
    public class VerificationCertificates : IVerificationCertificates
    {
        private readonly Dictionary<string, byte[]> _certificates;

        public VerificationCertificates()
        {
            _certificates = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void Add(string baseName, byte[] certificate)
        {
            _certificates.Add(baseName, certificate);
        }

        public bool Contains(string baseName)
        {
            return _certificates.ContainsKey(baseName);
        }

        public byte[] Get(string baseName)
        {
            return _certificates[baseName];
        }
    }
}
