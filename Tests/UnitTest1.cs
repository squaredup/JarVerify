using System;
using System.IO;
using JarVerify.Cryptography;
using JarVerify.Container;
using JarVerify.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JarVerify;
using System.Text;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(SigningStatus.SignedValid, Verify.Jar(@"C:\Users\David\Downloads\jenkins.war.zip"));

            //using (IJar j = new Jar(@"C:\Users\David\Downloads\jenkins.war.zip"))
            //{
            //    Assert.IsTrue(j.Contains(@"css\google-fonts\roboto\fonts\roboto-v15-greek_latin-ext_latin_vietnamese_cyrillic_greek-ext_cyrillic-ext-300.eot"));

            //    ///Console.WriteLine(new StreamReader(j.OpenFile(@"css\color.css")).ReadToEnd());
            //    ///

            //    string test = "Name: WEB-INF/security/SecurityFilters.groovy" + Environment.NewLine
            //    + "SHA-256-Digest: wutk4867i4dBQglc8LP+FA7mWC1++m0nn00oQ2TOWoA=" + Environment.NewLine
            //    + Environment.NewLine;
                

            //    using (Hasher h = new Hasher())
            //    {
            //        Console.WriteLine(h.SHA256(Encoding.UTF8.GetBytes(test)).ToBase64());

            //        foreach (string file in j.Files())
            //        {
            //            Console.WriteLine($"{file}  {j.SHA256(h, file).ToBase64()}");
            //        }
            //    }
            //}
        }
    }
}
