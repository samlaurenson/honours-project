using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ActressMas;
using HonoursProject;
using HonoursProject.behaviours;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HonoursProject.Tests
{
    [TestClass()]
    public class AgentEnvironmentTests
    {
        [TestMethod()]
        public void AgentInteractionAndEnvironmentTest()
        {
            //Unit test to ensure that agent interactions are working as expected.
            //If agent interactions are working - then unit test should terminate, otherwise unit test will execute indefinitely
            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();
            datastore.SimulationData.Clear();
            datastore.EndOfDaySatisfaction.Clear();

            List<int> evolving = new List<int>() { 9 };
            List<int> exchanges = new List<int>() { 50 };

            var results = Program.InitialiseEnvironment(evolving, exchanges, 1, 24, 50, 16, 15);

            //If environment worked correctly - then there should be some data stored in this list
            Assert.IsTrue(results[9][50][0].Count > 0);
        }

        [TestMethod()]
        public void EnvTestWithNoHouseAgents()
        {
            //Unit test to ensure that model will not run when there are no house agents present in the model
            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();
            datastore.SimulationData.Clear();
            datastore.EndOfDaySatisfaction.Clear();

            List<int> evolving = new List<int>() { 9 };
            List<int> exchanges = new List<int>() { 50 };

            var results = Program.InitialiseEnvironment(evolving, exchanges, 1, 24, 0, 16, 15);

            //If no house agents added to environment - function will return null
            Assert.IsNull(results);
        }

        [TestMethod()]
        public void EnvTestDayManagerNilParameters()
        {
            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();
            datastore.SimulationData.Clear();
            datastore.EndOfDaySatisfaction.Clear();

            List<int> evolving = new List<int>() { 0 };
            List<int> exchanges = new List<int>() { 50 };

            var results = Program.InitialiseEnvironment(evolving, exchanges, 1, 24, 50, 16, 0);

            //If there are no days in the model - model will produce data, meaning no data will be stored for the end of the day
            Assert.IsNull(results);
        }

        [TestMethod()]
        public void EnvTestAdvertiserNilParameters()
        {
            //Unit test to ensure that model will not run when there are no house agents present in the model
            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();
            datastore.SimulationData.Clear();
            datastore.EndOfDaySatisfaction.Clear();

            List<int> evolving = new List<int>() { 9 };
            List<int> exchanges = new List<int>() { 0 };

            var results = Program.InitialiseEnvironment(evolving, exchanges, 1, 24, 50, 16, 15);

            //No exchange rounds in model should still produce data
            Assert.IsTrue(results[9][0][0].Count > 0);
        }

        [TestMethod()]
        public void EnvTestNullInputs()
        {
            //Unit test to ensure that model will not run when there are no house agents present in the model
            DataStore datastore = DataStore.Instance;
            datastore.HouseAgents.Clear();
            datastore.SimulationData.Clear();
            datastore.EndOfDaySatisfaction.Clear();

            List<int> evolving = new List<int>() { 9 };
            List<int> exchanges = new List<int>() { 0 };

            var results = Program.InitialiseEnvironment(null, null, 1, 24, 50, 16, 15);

            //No exchange rounds in model should still produce data
            Assert.IsNull(results);
        }
    }
}
