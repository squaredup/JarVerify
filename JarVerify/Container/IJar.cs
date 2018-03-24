using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarVerify.Container
{
    /// <summary>
    /// Interaction with the contents of a JAR 
    /// </summary>
    public interface IJar : IDisposable
    {
        /// <summary>
        /// Open the data contained within the given relative JAR path
        /// </summary>
        /// <param name="path">path to the file to open</param>
        /// <returns>the raw data stream of the file, or null if no such file exists</returns>
        Stream Open(string path);

        /// <summary>
        /// Whether the given filename exists in the JAR
        /// </summary>
        /// <param name="path">path to test for existence</param>
        /// <returns>whether or not the JAR contains this filename</returns>
        bool Contains(string path);

        /// <summary>
        /// Get the list of all relative file paths inside the JAR file. This includes only files - 
        /// folder path names are omitted
        /// </summary>
        /// <returns>the list of all file paths inside the JAR</returns>
        IEnumerable<string> Files();
    }
}
