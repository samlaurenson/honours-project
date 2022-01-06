using Microsoft.VisualStudio.TestTools.UnitTesting;
using HonoursProject.behaviours;
using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours.Tests
{
    [TestClass()]
    public class SelfishBehaviourTests
    {
        [TestMethod()]
        public void ConsiderRequestTest_MutuallyBeneficial()
        {
            var agent = new HouseAgent(new SelfishBehaviour());
            var agent2 = new HouseAgent(new SelfishBehaviour());

            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 6, 3, 8 };

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsTrue(decision);
        }

        [TestMethod()]
        public void ConsiderRequestTest_AgentDeniesRequest()
        {
            var agent = new HouseAgent(new SelfishBehaviour());
            var agent2 = new HouseAgent(new SelfishBehaviour());

            //"agent" will not want the requesting agent slot as the agent does not want slot 6
            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 9, 3, 8 };

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsFalse(decision);
        }
    }
}