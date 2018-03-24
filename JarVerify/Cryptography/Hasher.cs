using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Cryptography
{
    public class Hasher : IDisposable
    {
        private readonly SHA256 _sha;

        public Hasher()
        {
            _sha = SHA256Managed.Create();
        }

        public byte[] SHA256(string str)
        {
            return SHA256(Encoding.UTF8.GetBytes(str));
        }

        public byte[] SHA256(byte[] bytes)
        {
            return _sha.ComputeHash(bytes);
        }

        public byte[] SHA256(Stream file)
        {
            return _sha.ComputeHash(file);
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _sha.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Hasher() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
