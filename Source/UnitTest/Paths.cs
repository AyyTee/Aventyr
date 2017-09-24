using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public static class Paths
    {
        public static string Data => Path.Combine(TestContext.CurrentContext.TestDirectory, "Data");
    }
}
