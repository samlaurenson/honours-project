using Microsoft.VisualStudio.TestTools.UnitTesting;
using HonoursProject.behaviours;
using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours.Tests
{
    [TestClass()]
    public class SocialBehaviourTests
    {
        [TestMethod()]
        public void ConsiderRequestTest_MutuallyBeneficial()
        {
            var agent = new HouseAgent(new SocialBehaviour(), 0);
            var agent2 = new HouseAgent(new SocialBehaviour(), 1);

            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 6, 3, 8 };
            agent.SocialCapital = true;

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsTrue(decision);
        }

        [TestMethod()]
        public void ConsiderRequestTest_AgentDeniesRequest()
        {
            var agent = new HouseAgent(new SocialBehaviour(), 0);
            var agent2 = new HouseAgent(new SocialBehaviour(), 1);

            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 9, 3, 8 };
            agent.SocialCapital = true;

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsFalse(decision);
        }

        [TestMethod()]
        public void ConsiderRequestTest_AgentAcceptedBecauseFavourOwed()
        {
            var agent = new HouseAgent(new SocialBehaviour(), 0);
            var agent2 = new HouseAgent(new SocialBehaviour(), 1);

            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 9, 3, 8 };
            agent.SocialCapital = true;
            agent.FavoursOwed.Add("agent2", 1);

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsTrue(decision);
        }

        [TestMethod()]
        public void ConsiderRequestTest_AgentAcceptedNoSocialCapital()
        {
            var agent = new HouseAgent(new SocialBehaviour(), 0);
            var agent2 = new HouseAgent(new SocialBehaviour(), 1);

            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 9, 3, 8 };
            agent.SocialCapital = false;
            agent.FavoursOwed.Add("agent2", 1);

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsTrue(decision);
        }

        [TestMethod()]
        public void ConsiderRequestTest_AgentDeclinesUsingSocialCapital()
        {
            var agent = new HouseAgent(new SocialBehaviour(), 0);
            var agent2 = new HouseAgent(new SocialBehaviour(), 1);

            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 9, 3, 8 };
            agent.SocialCapital = true;
            agent.FavoursOwed.Add("agent2", 1);
            agent.FavoursGiven.Add("agent2", 2);

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsFalse(decision);
        }
    }
}