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
    /// Errors relating to the JAR file itself - reading, opening etc.
    /// </summary>
    [Serializable]
    public class JarException : Exception
    {
        public JarException()
        {
        }

        public JarException(string message)
            : base(message)
        {
        }

        public JarException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected JarException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
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
