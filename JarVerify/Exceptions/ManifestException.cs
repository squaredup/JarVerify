using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Exceptions
{
    /// <summary>
    /// Something being incorrect about a manifest
    /// </summary>
    [Serializable]
    public class ManifestException : Exception
    {
        public ManifestException()
        {
        }

        public ManifestException(string message)
            : base(message)
        {
        }

        public ManifestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ManifestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
        }
    }
}
