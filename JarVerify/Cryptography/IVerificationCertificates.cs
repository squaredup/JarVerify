using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Cryptography
{
    public interface IVerificationCertificates
    {
        void Add(string baseName, byte[] certificate);

        bool Contains(string baseName);

        byte[] Get(string baseName);
    }
}
