using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Container
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
        /// The path to the .SF manifest within the JAR for this signature
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

    /// <summary>
    /// Find a set of signatures within a JAR
    /// </summary>
    public interface ISignatureFinder
    {
        /// <summary>
        /// Detect a list of all signatures within a JAR
        /// </summary>
        /// <param name="jar">JAR to search</param>
        /// <returns>set of all detected signatures</returns>
        List<Signature> Find(IJar jar);
    }
}
