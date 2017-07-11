using System;
using Game.Common;
using NUnit.Framework;

[SetUpFixture]
public class Config
{
    [OneTimeSetUp]
    public void SetUp()
    {
        DebugEx.FailEvent += message => Assert.Fail(message);
    }
}
