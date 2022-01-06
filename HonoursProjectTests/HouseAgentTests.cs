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
    public class HouseAgentTests
    {
        [TestMethod()]
        public void CalculateSatisfactionTest_WithNullFunctionInput()
        {
            var agent = new HouseAgent(new SelfishBehaviour());
            agent.AllocatedSlots = new List<int>() { 1, 2, 3, 4 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };

            Assert.AreEqual(0.9375, agent.CalculateSatisfaction(null));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_WithValidFunctionInput()
        {
            var agent = new HouseAgent(new SelfishBehaviour());
            List<int> allocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };
            Assert.AreEqual(0.6875, agent.CalculateSatisfaction(allocatedSlots));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_WithEmptyListInput()
        {
            var agent = new HouseAgent(new SelfishBehaviour());
            List<int> allocatedSlots = new List<int>();
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };
            Assert.AreEqual(0, agent.CalculateSatisfaction(allocatedSlots));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_WithBothEmptyLists()
        {
            var agent = new HouseAgent(new SelfishBehaviour());
            agent.AllocatedSlots = new List<int>();
            agent.RequestedSlots = new List<int>();
            Assert.AreEqual(0, agent.CalculateSatisfaction(null));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_EmptyRequestSlots()
        {
            var agent = new HouseAgent(new SelfishBehaviour());
            List<int> allocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>();
            Assert.AreEqual(0, agent.CalculateSatisfaction(allocatedSlots));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_InvalidRequestTimeSlots()
        {
            var agent = new HouseAgent(new SelfishBehaviour());
            List<int> allocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 0, 2, 3, 24 };
            Assert.AreEqual(0.625, agent.CalculateSatisfaction(allocatedSlots));
        }
    }
}