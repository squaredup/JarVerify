using System;
using System.IO;
using JarVerify.Cryptography;
using JarVerify.Jar;
using JarVerify.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (IJar j = new Jar(@"C:\Users\David\Downloads\jenkins.war.zip"))
            {
                Assert.IsTrue(j.Contains(@"css\google-fonts\roboto\fonts\roboto-v15-greek_latin-ext_latin_vietnamese_cyrillic_greek-ext_cyrillic-ext-300.eot"));

                ///Console.WriteLine(new StreamReader(j.OpenFile(@"css\color.css")).ReadToEnd());
                ///

                using (Hasher h = new Hasher())
                {
                    foreach (string file in j.Files())
                    {
                        Console.WriteLine($"{file}  {j.SHA256(h, file).ToBase64()}");
                    }
                }
            }
        }
    }
}
