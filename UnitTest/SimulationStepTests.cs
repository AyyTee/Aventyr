using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game.Portals;
using Game;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class SimulationStepTests
    {
        #region Step tests
        [TestMethod]
        public void StepTest0()
        {
            Portalable p = new Portalable();
            Transform2 start = new Transform2(new Vector2(1, 5), 2.3f, 3.9f);
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(-3, 4), 23, 0.54f);
            p.SetTransform(start);
            p.SetVelocity(velocity);
            SimulationStep.Step(new IPortalable[] { p }, new IPortal[0], 1, null);

            Assert.IsTrue(p.GetTransform().AlmostEqual(start.Add(velocity)));
        }

        /// <summary>
        /// Portal and portalable shouldn't collide so the result should be the same as in StepTest0.
        /// </summary>
        [TestMethod]
        public void StepTest1()
        {
            Portalable p = new Portalable();
            Transform2 start = new Transform2(new Vector2(1, 5), 2.3f, 3.9f);
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(-3, 4), 23, 0.54f);
            p.SetTransform(start);
            p.SetVelocity(velocity);

            Scene scene = new Scene();
            FloatPortal portal = new FloatPortal(scene);

            SimulationStep.Step(new IPortalable[] { p }, new IPortal[] { portal }, 1, null);

            Assert.IsTrue(p.GetTransform().AlmostEqual(start.Add(velocity)));
        }

        [TestMethod]
        public void StepTest2()
        {
            Portalable p = new Portalable();
            Transform2 start = new Transform2(new Vector2(0, 0));
            Transform2 velocity = Transform2.CreateVelocity(new Vector2(3, 0));
            p.SetTransform(start);
            p.SetVelocity(velocity);

            Scene scene = new Scene();
            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(1, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(10, 10)));

            enter.Linked = exit;
            exit.Linked = enter;

            SimulationStep.Step(new IPortalable[] { p }, new IPortal[] { enter, exit }, 1, null);

            Assert.IsTrue(p.GetTransform().Position == new Vector2(-1, 10));
        }
        #endregion
    }
}
