using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Manifest
{
    /// <summary>
    /// Information about an individual signature (one of possibly many)
    /// </summary>
    public class Signature
    {
        /// <summary>
        /// Base name of the signature (e.g. (BASENAME).SF))
        /// </summary>
        public string BaseName
        {
            get;
            set;
        }

        /// <summary>
        /// The path to the .SF manifest for this signature
        /// </summary>
        public string ManifestPath
        {
            get;
            set;
        }

        /// <summary>
        /// Details of where the signature block file resides
        /// </summary>
        public SignatureBlockFile Block
        {
            get;
            set;
        }
    }

    public enum SignatureBlockType
    {
        /// <summary>
        /// PKCS7 signature, SHA-256 + RSA
        /// </summary>
        RSA,

        /// <summary>
        /// PKCS7 signature, DSA
        /// </summary>
        DSA
    }

    public class SignatureBlockFile
    {
        public SignatureBlockType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Path to the block file
        /// </summary>
        public string Path
        {
            get;
            set;
        }
    }
}
