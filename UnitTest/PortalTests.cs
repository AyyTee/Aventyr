using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class PortalTests
    {
        [TestMethod]
        public void IsInsideFOVTest0()
        {
            for (double i = 0; i < Math.PI * 2; i += Math.PI/10)
            {
                Portal p0 = new Portal();
                p0.Transform.Rotation = (float) (i + Math.PI/4);
                Vector2 viewPoint = new Vector2((float) Math.Cos(i + Math.PI), (float) Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2((float) Math.Cos(i), (float) Math.Sin(i));
                Assert.IsTrue(p0.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest1()
        {
            float x = 100;
            float y = 100;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Assert.IsTrue(p0.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest2()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x, y);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Assert.IsTrue(p0.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest3()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x, y);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Assert.IsFalse(p0.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest4()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i + Math.PI), y + (float)Math.Sin(i + Math.PI));
                Vector2 lookPoint = new Vector2(x, y);
                Assert.IsFalse(p0.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest5()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Vector2 lookPoint = new Vector2(x, y);
                Assert.IsTrue(p0.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest6()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i) * 2, y + (float)Math.Sin(i) * 2);
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Assert.IsFalse(p0.IsInsideFOV(viewPoint, lookPoint));
            }
        }
        [TestMethod]
        public void IsInsideFOVTest7()
        {
            float x = 0;
            float y = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 10)
            {
                Portal p0 = new Portal(new Vector2(x, y));
                p0.Transform.Rotation = (float)(i + Math.PI / 4);
                Vector2 viewPoint = new Vector2(x + (float)Math.Cos(i), y + (float)Math.Sin(i));
                Vector2 lookPoint = new Vector2(x + (float)Math.Cos(i) * 2, y + (float)Math.Sin(i) * 2);
                Assert.IsFalse(p0.IsInsideFOV(viewPoint, lookPoint));
            }
        }
    }
}
