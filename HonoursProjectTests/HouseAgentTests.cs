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
        public void HouseAgent_NullInput()
        {
            //Testing that if behaviour is not input - exception is thrown
            Assert.ThrowsException<Exception>(() => new HouseAgent(null, 1));
        }

        [TestMethod()]
        public void HouseAgent_InvalidID()
        {
            //Testing that if agent has an invalid ID - exception is thrown
            Assert.ThrowsException<Exception>(() => new HouseAgent(new SocialBehaviour(), -100));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_WithNullFunctionInput()
        {
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AllocatedSlots = new List<int>() { 1, 2, 3, 4 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };

            Assert.AreEqual(0.75, agent.CalculateSatisfaction(null));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_WithValidFunctionInput()
        {
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            List<int> allocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };
            Assert.AreEqual(0.5, agent.CalculateSatisfaction(allocatedSlots));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_WithEmptyListInput()
        {
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            List<int> allocatedSlots = new List<int>();
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };
            Assert.AreEqual(0, agent.CalculateSatisfaction(allocatedSlots));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_WithBothEmptyLists()
        {
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>();
            agent.RequestedSlots = new List<int>();
            Assert.AreEqual(0, agent.CalculateSatisfaction(null));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_EmptyRequestSlots()
        {
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            List<int> allocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>();
            Assert.AreEqual(0, agent.CalculateSatisfaction(allocatedSlots));
        }

        [TestMethod()]
        public void CalculateSatisfactionTest_InvalidRequestTimeSlots()
        {
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            List<int> allocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 0, 2, 3, 25 };
            Assert.AreEqual(0.25, agent.CalculateSatisfaction(allocatedSlots));
        }
    }
}