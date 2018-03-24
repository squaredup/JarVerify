using JarVerify.Exceptions;
using JarVerify.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Container
{
    /// <summary>
    /// An actual JAR (ZIP) implementation. We are always read-only: verifying a signature 
    /// never requires update or write
    /// </summary>
    public class Jar : IJar
    {   
        private ZipArchive _zip;
        
        public Jar(string filename)
        {
            if (filename.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(filename));
            }

            Stream file;

            try
            {
                // By default ZipArchive kills the stream the data comes from, so
                // we do not have to cleanup the file stream 
                file = new FileStream(filename, FileMode.Open);
            }
            catch(Exception ex)
            {
                throw new JarException($"Failed to open JAR source file {filename}", ex);
            }

            Open(file);
        }

        public Jar(Stream stream, bool leaveStreamOpen = false)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Open(stream, leaveStreamOpen);
        }

        private void Open(Stream stream, bool leaveStreamOpen = false)
        {
            try
            {
                _zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveStreamOpen);
            }
            catch(Exception ex)
            {
                throw new JarException("Failed to open JAR ZIP data for reading", ex);
            }
        }

        private ZipArchiveEntry GetEntry(string path)
        {
            string normalized = path.ToForwardSlashes();

            // We do this rather than built-in get entry to be case insensitive
            //
            // TODO: ZIP always seems to read entries as forward slashes, is this alaways the case?
            // TODO: Culture for UTF-8 filenames?
            //
            return _zip.Entries.FirstOrDefault(e => e.FullName.ToForwardSlashes()
                .Equals(normalized, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool Contains(string path)
        {
            if (path.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetEntry(path) != null;
        }

        public Stream Open(string path)
        {
            if (path.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(path));
            }

            ZipArchiveEntry entry = GetEntry(path);

            if (entry != null)
            {
                return entry.Open();
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<string> Files()
        {
            return _zip.Entries
                .Where(e => e.Length != 0)
                .Select(e => e.FullName);
        }

        #region IDisposable Support
        private bool _dsiposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_dsiposed)
            {
                if (disposing)
                {
                    if (_zip != null)
                    {
                        _zip.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _dsiposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Jar() {
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
