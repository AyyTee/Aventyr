using Game.Common;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using TimeLoopInc;

namespace GameTests
{
    [TestFixture]
    public class SceneStateTests
    {
        [Test]
        public void DeepCloneTest0()
        {
            var sceneState = new SceneState();
            var clone = sceneState.DeepClone();

            AreEqual(sceneState, clone);
        }

        [Test]
        public void DeepCloneTest1()
        {
            var sceneState = new SceneState();
            var blockTimeline = new Timeline<Block>();
            blockTimeline.Add(new Block(new Vector2i(1, 1), 2, 2));
            sceneState.BlockTimelines.Add(blockTimeline);
            var clone = sceneState.DeepClone();

            AreEqual(sceneState, clone);
        }

        static void AreEqual(SceneState expected, SceneState result)
        {
            Assert.IsFalse(ReferenceEquals(expected, result));
            using (var stream0 = new MemoryStream())
            using (var stream1 = new MemoryStream())
            {
                new Serializer().Serialize(stream0, expected);
                new Serializer().Serialize(stream1, result);

                Assert.IsTrue(CompareMemoryStreams(stream0, stream1));
            }
        }

        static bool CompareMemoryStreams(MemoryStream ms1, MemoryStream ms2)
        {
            if (ms1.Length != ms2.Length)
                return false;
            ms1.Position = 0;
            ms2.Position = 0;

            var msArray1 = ms1.ToArray();
            var msArray2 = ms2.ToArray();

            return msArray1.SequenceEqual(msArray2);
        }
    }
}
