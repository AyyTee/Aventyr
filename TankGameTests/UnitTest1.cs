using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TankGame.Network;

namespace TankGameTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestInitialize]
        public void CreateClient()
        {
            Client client = new Client(null, new FakeController(), new FakeNetClient());
        }

        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}
