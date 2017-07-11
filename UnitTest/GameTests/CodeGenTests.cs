using Game;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTests
{
    [TestFixture]
    public class CodeGenTests
    {
        [Test]
        public void ToSingleTest0()
        {
            var result = GenerateSingles.ToSingle(" Transform2d ");
            Assert.AreEqual(" Transform2 ", result);
        }

        [Test]
        public void ToSingleTest1()
        {
            var input = " tTransform2d ";
            var result = GenerateSingles.ToSingle(input);
            Assert.AreEqual(input, result);
        }
    }
}
