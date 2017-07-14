using System;
using Game.Common;
using NUnit.Framework;

[SetUpFixture]
public class Config
{
    [OneTimeSetUp]
    public void SetUp()
    {
        DebugEx.FailEvent += Assert.Fail;
    }
}
