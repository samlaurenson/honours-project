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
        public void CalculateSatisfactionTest_AgentInteractionAndEnvironmentTest()
        {
            //Unit test to ensure that agent interactions are working as expected.
            //If agent interactions are working - then unit test should terminate, otherwise unit test will execute indefinitely
            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();
            datastore.SimulationData.Clear();
            datastore.EndOfDaySatisfaction.Clear();

            var env = new EnvironmentMas();

            for (int k = 0; k < 25 / 2; k++)
            {
                var houseAg = new HouseAgent(new SelfishBehaviour(), k);
                houseAg.SocialCapital = true;
                datastore.HouseAgents.Add(houseAg);
                env.Add(houseAg, $"house{k}");
            }

            for (int k = 25 / 2; k < 25; k++)
            {
                var houseAg = new HouseAgent(new SocialBehaviour(), k);
                houseAg.SocialCapital = true;
                datastore.HouseAgents.Add(houseAg);
                env.Add(houseAg, $"house{k}");
            }

            var day = new DayManagerAgent(9, 5);
            var ad = new AdvertisingAgent(10);
            env.Memory.Add("UniqueTimeSlots", 24);
            env.Memory.Add("NoOfAgents", 2);
            env.Memory.Add("MaxSlotCapacity", 16);
            env.Add(day, "daymanager");
            env.Add(ad, "advertiser");
            env.Start();

            //If environment worked correctly - then there should be some agent satisfactions stored in this list
            Assert.IsNotNull(datastore.EndOfDaySatisfaction);
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
            var agent = new HouseAgent(new SocialBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 6, 3, 4 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };

            //Swapping slot 4 in allocated slots for slot 5 (the slot that the desired agent had)
            agent.HandleAcceptedRequest(4, 5, "agent1");

            //Swapping slot 6 for slot 2 -- repeating this to test the favours owed dictionary works correctly
            agent.HandleAcceptedRequest(6, 2, "agent1");

            //Slots added in at end of array since the element of current slot index is not modified but removed completely, then desired slot is added in at end
            CollectionAssert.AreEqual(new List<int>() { 1, 3, 5, 2 }, agent.AllocatedSlots);
        }

        [TestMethod()]
        public void SlotToRequestTest()
        {
            //Test to ensure agent will select a slot to request that will bring them satisfaction
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 3 };
            agent.RequestedSlots = new List<int>() { 1, 2, 3, 5 };

            List<int> advertisedSlots = new List<int>() { 5, 7, 8 };

            Tuple<int?, int?> outputTuple = agent.SlotToRequest(advertisedSlots);

            //Desired slot -> 5, proposed slot to exchange for desired -> 7
            Tuple<int?, int?> expectedTuple = new Tuple<int?, int?>(5, 7);

            string actual = $"{outputTuple.Item1} {outputTuple.Item2}";
            string expected = $"{expectedTuple.Item1} {expectedTuple.Item2}";

            Assert.AreEqual(expected, actual);
        }
    }
}