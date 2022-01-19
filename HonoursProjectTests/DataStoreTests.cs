using Microsoft.VisualStudio.TestTools.UnitTesting;
using HonoursProject;
using System;
using System.Collections.Generic;
using System.Text;
using ActressMas;
using HonoursProject.behaviours;

namespace HonoursProject.Tests
{
    [TestClass()]
    public class DataStoreTests
    {
        [TestMethod()]
        public void CalculateEndOfDaySatisfactionsTest()
        {
            var sel = new HouseAgent(new SelfishBehaviour(), 0);
            sel.AgentFlexibility = new List<double>() { 1.0 };
            sel.AllocatedSlots = new List<int>() { 1, 6, 3, 4 };
            sel.RequestedSlots = new List<int>() { 1, 4, 3, 5 };
            sel.Name = "house0";

            var soc = new HouseAgent(new SocialBehaviour(), 1);
            soc.AgentFlexibility = new List<double>() { 1.0 };
            soc.AllocatedSlots = new List<int>() { 2, 5, 7, 8 };
            soc.RequestedSlots = new List<int>() { 1, 2, 7, 5 };
            soc.Name = "house1";


            DataStore dataStore = DataStore.Instance;

            dataStore.HouseAgents.Clear();
            dataStore.EndOfDaySatisfaction.Clear();

            dataStore.HouseAgents.Add(sel);
            dataStore.HouseAgents.Add(soc);

            dataStore.CalculateEndOfDaySatisfactions(0);

            List<List<double>> expected = new List<List<double>>();
            expected.Add(new List<double>()
            {
                0.75,
                0.75,
                0.75,
                0.0,
                0.75,
                0
            });

            CollectionAssert.AreEqual(expected[0], dataStore.EndOfDaySatisfaction[0]);
        }
    }
}