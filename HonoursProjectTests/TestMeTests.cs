using Microsoft.VisualStudio.TestTools.UnitTesting;
using HonoursProject;
using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.Tests
{
    [TestClass()]
    public class TestMeTests
    {
        [TestMethod()]
        public void funcTest()
        {
            TestMe t = new TestMe();
            Assert.AreEqual(t.func(2), 3);
        }
    }
}