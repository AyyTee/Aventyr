using System;
using Game.Common;
using NUnit.Framework;
using Game;
using System.IO;
using Game.Serialization;

[SetUpFixture]
public class Config
{
    public static Resources Resources { get; private set; }

    [OneTimeSetUp]
    public void SetUp()
    {
        DebugEx.FailEvent += Assert.Fail;

        var resourcePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory, 
            Resources.ResourcePath, 
            "Assets.json");
        var data = File.ReadAllText(resourcePath);
        Resources = Serializer.Deserialize<Resources>(data);
    }
}
