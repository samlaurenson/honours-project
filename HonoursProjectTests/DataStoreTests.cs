﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            sel.RequestedSlots = new List<int>() { 1, 2, 3, 5 };
            sel.Name = "house0";

            var soc = new HouseAgent(new SocialBehaviour(), 1);
            soc.AgentFlexibility = new List<double>() { 1.0 };
            soc.AllocatedSlots = new List<int>() { 1, 6, 3, 4 };
            soc.RequestedSlots = new List<int>() { 1, 2, 3, 5 };
            soc.Name = "house1";


            DataStore dataStore = DataStore.Instance;
            dataStore.HouseAgents.Add(sel);
            dataStore.HouseAgents.Add(soc);

            dataStore.CalculateEndOfDaySatisfactions(0);

            List<List<double>> expected = new List<List<double>>();
            expected.Add(new List<double>()
            {
                0.125,
                0.625,
                0.5,
                0.4629100498862757,
                0.5,
                0
            });

            CollectionAssert.AreEqual(expected[0], dataStore.EndOfDaySatisfaction[0]);
        }
    }
}