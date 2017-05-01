using Game;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTests
{
    [TestClass]
    public class CodeGenTests
    {
        [TestMethod]
        public void ToSingleTest0()
        {
            var result = GenerateSingles.ToSingle(" Transform2d ");
            Assert.AreEqual(" Transform2 ", result);
        }

        [TestMethod]
        public void ToSingleTest1()
        {
            var input = " tTransform2d ";
            var result = GenerateSingles.ToSingle(input);
            Assert.AreEqual(input, result);
        }
    }
}
