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
        /*
         *
         *      Testing considering request function
         *
         */

        [TestMethod()]
        public void ConsiderRequestTest_MutuallyBeneficial()
        {
            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();
            datastore.SimulationData.Clear();
            datastore.EndOfDaySatisfaction.Clear();

            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            var agent2 = new HouseAgent(new SelfishBehaviour(), 1);

            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 6, 3, 8 };
            agent.Name = "agent0";

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };
            agent2.Name = "agent2";

            datastore.HouseAgents.Add(agent);
            datastore.HouseAgents.Add(agent2);

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsTrue(decision);
        }

        [TestMethod()]
        public void ConsiderRequestTest_AgentDeniesRequest()
        {
            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();
            datastore.SimulationData.Clear();
            datastore.EndOfDaySatisfaction.Clear();
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            var agent2 = new HouseAgent(new SelfishBehaviour(), 1);

            //"agent" will not want the requesting agent slot as the agent does not want slot 6 (6 is not in agent requested slot list)
            agent.AgentFlexibility = new List<double>() { 1.0 };
            agent.AllocatedSlots = new List<int>() { 1, 2, 7, 4 };
            agent.RequestedSlots = new List<int>() { 5, 9, 3, 8 };
            agent.Name = "agent0";

            agent2.AgentFlexibility = new List<double>() { 1.0 };
            agent2.AllocatedSlots = new List<int>() { 6, 9, 7, 3 };
            agent2.RequestedSlots = new List<int>() { 1, 2, 4, 5 };
            agent2.Name = "agent2";

            datastore.HouseAgents.Add(agent);
            datastore.HouseAgents.Add(agent2);

            bool decision = agent.Behaviour.ConsiderRequest(agent, "agent2", 6, 4);
            Assert.IsFalse(decision);
        }

        /*
         *
         *      Testing switch strategy function
         *
         */

        [TestMethod()]
        public void SwitchStrategy_SelfishToSocial()
        {
            var agent = new HouseAgent(new SelfishBehaviour(), 0);
            agent.Behaviour.SwitchStrategy(agent);
            Assert.IsTrue(agent.Behaviour is SocialBehaviour);
        }
    }
}