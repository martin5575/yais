using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yais.Model.Search.ContentFinder;

namespace Yias.UnitTest
{
    [TestClass]
    public class NameFinderTest
    {
        private NameFinder _target = new NameFinder();

        [TestMethod]
        public void TestEasyName()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Michael Meier", out found));
            Assert.AreEqual("Michael Meier", found.Content);
            Assert.AreEqual(FoundContentType.Name, found.Type);
        }

        [TestMethod]
        public void TestGermanUmlauteName()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Jörg Voß", out found));
            Assert.AreEqual("Jörg Voß", found.Content);
        }

        [TestMethod]
        public void TestLongName()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Rubens de la Rocha", out found));
            Assert.AreEqual("Rubens de la Rocha", found.Content);
        }


        [TestMethod]
        public void TestComplexFirstName()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Marc-Andre ter Stegen", out found));
            Assert.AreEqual("Marc-Andre ter Stegen", found.Content);
        }

        [TestMethod]
        public void TestComplexLastName()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Simone Wiczorek-Zoll", out found));
            Assert.AreEqual("Simone Wiczorek-Zoll", found.Content);
        }

        [TestMethod]
        public void TestVeryComplexLastName()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Edson Arantes do Nascimento", out found));
            Assert.AreEqual("Edson Arantes do Nascimento", found.Content);
        }

        [TestMethod]
        public void TestVeryComplexLastName2()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Renato Gonzales de la Galvo", out found));
            Assert.AreEqual("Renato Gonzales de la Galvo", found.Content);
        }

        [TestMethod]
        public void TestGermanUmaluteName2()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Ümet Özcalan", out found));
            Assert.AreEqual("Ümet Özcalan", found.Content);
        }

        [TestMethod]
        public void TestFailure()
        {
            FoundContent found;
            Assert.IsFalse(_target.TryFind("12 Polizei 34 Grenadier", out found));
        }

        [TestMethod]
        public void TestFailure2()
        {
            FoundContent found;
            Assert.IsFalse(_target.TryFind("Dieser Test stinkt gewaltig", out found));
        }

        [TestMethod]
        public void TestWithinText()
        {
            FoundContent found;
            Assert.IsTrue(_target.TryFind("Geschäftsführer: Dr.Thomas Baumann, Ingo Müller", out found));
            Assert.AreEqual("Thomas Baumann", found.Content);
            Assert.AreEqual(FoundContentType.Name, found.Type);
        }


    }
}
