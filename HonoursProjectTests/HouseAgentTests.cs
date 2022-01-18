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
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0, 0.75 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 3, 4 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };

            Assert.AreEqual(0.9375, agent.CalculateSatisfaction(null));
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
            agent.AgentFlexibility = new List<double>() { 1.0, 0.75 };
            List<int> allocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 0, 2, 3, 24 };
            Assert.AreEqual(0.625, agent.CalculateSatisfaction(allocatedSlots));
        }

        [TestMethod()]
        public void ListUnwantedSlotsTest()
        {
            //Checking that the house agent is listing the correct slots - being the slots that it does not want
            //Unwanted slots are slots in the allocated list that are not in the requested list
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };

            string slotCheck = agent.ListUnwantedSlots();

            //Slots in allocated list that are not in requested list are slots 7 and 4
            Assert.AreEqual("7 4", slotCheck);
        }

        [TestMethod()]
        public void ListUnwantedSlotsTest_NoUnwanted()
        {
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 3, 5 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };

            string slotCheck = agent.ListUnwantedSlots();

            Assert.AreEqual("", slotCheck);
        }

        [TestMethod()]
        public void HandleAcceptRequest()
        {
            //Checking that the slots are being replaced properly when an exchange has been successful
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 3, 4 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };

            //Swapping slot 4 in allocated slots for slot 5 (the slot that the desired agent had)
            agent.HandleAcceptedRequest(4, 5, "agent1");

            CollectionAssert.AreEqual(new List<int>() { 1, 2, 3, 5 }, agent.AllocatedSlots);
        }
    }
}