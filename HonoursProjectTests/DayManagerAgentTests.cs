using Microsoft.VisualStudio.TestTools.UnitTesting;
using HonoursProject.behaviours;
using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours.Tests
{
    [TestClass()]
    public class DayManagerAgentTests
    {
        [TestMethod()]
        public void EndOfDayManagerTest()
        {
            var sel = new HouseAgent(new SelfishBehaviour(), 0);
            sel.AgentFlexibility = new List<double>() { 1.0 };
            sel.AllocatedSlots = new List<int>() { 1, 6, 3, 4 };
            sel.RequestedSlots = new List<int>() { 1, 4, 3, 5 };
            sel.Name = "house0";

            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();

            datastore.HouseAgents.Add(sel);

            var dayman = new DayManagerAgent(0, 5);
            dayman.EndOfDayManager();

            Assert.AreEqual(1, dayman.CurrentDay); //Checking that the day has incremented correctly and is now at day 1 (2nd day in the model)
        }
    }
}