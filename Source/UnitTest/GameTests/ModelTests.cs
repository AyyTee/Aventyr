using FileFormatWavefront;
using FileFormatWavefront.Model;
using Game;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.GameTests
{
    [TestFixture]
    public class ModelTests
    {
        [Test]
        public void LoadObjTest0()
        {
                var fileResult = FileFormatObj.Load(Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "Box.obj"), false);
                var models = Game.Models.Model.FromWavefront(fileResult.Model, new FakeVirtualWindow());
        }
    }
}
