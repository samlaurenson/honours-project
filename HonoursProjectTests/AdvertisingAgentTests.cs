using Microsoft.VisualStudio.TestTools.UnitTesting;
using HonoursProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActressMas;
using HonoursProject.behaviours;

namespace HonoursProject.Tests
{
    [TestClass()]
    public class AdvertisingAgentTests
    {
        [TestMethod()]
        public void ListHouseTimeSlotsTest()
        {
            //Testing that unwanted slots listed by households are being added to the advertising dictionary properly.
            var advert = new AdvertisingAgent(3);

            List<string> unwantedTimeSlotsH0 = new List<string>() { "7", "3" };
            List<string> unwantedTimeSlotsH1 = new List<string>() { "2", "5" };

            advert.ListHouseTimeSlots("house0", unwantedTimeSlotsH0);
            advert.ListHouseTimeSlots("house1", unwantedTimeSlotsH1);

            Dictionary<string, List<int>> toAdvertise = new Dictionary<string, List<int>>() { 
                { "house0", new List<int>() { 7, 3 } },
                { "house1", new List<int>() { 2, 5 } }
            };

            //Turning dictionaries in to strings so they can be compared to check that function works properly
            string expected = String.Join(",", toAdvertise.OrderBy(key => key.Key).Select(kv => kv.Key + ":" + String.Join("|", kv.Value.OrderBy(v => v))));
            string actual = String.Join(",", advert.AdvertisedSlots.OrderBy(key => key.Key).Select(kv => kv.Key + ":" + String.Join("|", kv.Value.OrderBy(v => v))));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void SendRequestTest()
        {
            //Testing that requests for a valid time slot are being sent and that requests are correct

            //Initialising house agents 
            var house0 = new HouseAgent(new SelfishBehaviour(), 0);
            house0.Name = "house0";
            house0.AllocatedSlots = new List<int>() { 1, 2, 3, 4 };
            house0.RequestedSlots = new List<int>() { 6, 2, 3, 9 };

            var house1 = new HouseAgent(new SelfishBehaviour(), 1);
            house1.Name = "house1";


            //Adding houses to list of house agents in datastore
            DataStore dataStore = DataStore.Instance;
            dataStore.HouseAgents.Clear();
            dataStore.HouseAgents.Add(house0);
            dataStore.HouseAgents.Add(house1);


            //Setting up advertising agent
            var advert = new AdvertisingAgent(3);
            advert.CurrentAdvertisingHouse = "house0";

            advert.ListHouseTimeSlots("house0", new List<string>() {"1", "4"});

            advert.Requests.Add(new Tuple<string, int, int>("house1", 1, 6));   //House 1 sent request for slot 1 that House 0 has in exchange for slot 6
            
            string message = advert.RequestHandler();
            string expected = "sendRequest house1 6 1"; //Requesting agent, current slot (of requesting agent), desired slot

            Assert.AreEqual(expected, message);
        }

        [TestMethod()]
        public void SendRequestTest_ListLengthCheck()
        {
            //Tests that if request has been made - then the advertiser will be removed from list of advertising agents since they can only make 1 interaction per round

            //Initialising house agents 
            var house0 = new HouseAgent(new SelfishBehaviour(), 0);
            house0.Name = "house0";
            house0.AllocatedSlots = new List<int>() { 1, 2, 3, 4 };
            house0.RequestedSlots = new List<int>() { 6, 2, 3, 9 };
            house0.MadeInteraction = false;

            var house1 = new HouseAgent(new SelfishBehaviour(), 1);
            house1.Name = "house1";
            house1.MadeInteraction = false;

            var house2 = new HouseAgent(new SelfishBehaviour(), 1);
            house2.Name = "house2";
            house2.MadeInteraction = false;

            //Adding houses to list of house agents in datastore
            DataStore dataStore = DataStore.Instance;

            dataStore.HouseAgents.Clear();
            dataStore.HouseAgents.Add(house0);
            dataStore.HouseAgents.Add(house1);
            dataStore.HouseAgents.Add(house2);

            //Setting up advertising agent
            var advert = new AdvertisingAgent(3);
            advert.CurrentAdvertisingHouse = "house0";

            advert.ListHouseTimeSlots("house0", new List<string>() { "1", "4" });

            advert.Requests.Add(new Tuple<string, int, int>("house1", 1, 6));   //House 1 sent request for slot 1 that House 0 has in exchange for slot 6
            advert.Requests.Add(new Tuple<string, int, int>("house2", 4, 9));

            string message = advert.RequestHandler();

            //Advertising agent should be removed from list of agents to advertise for since they will not be able to make another interaction this round
            Assert.AreEqual(0, advert.AdvertisedSlots.Count);
        }

        [TestMethod()]
        public void SendRequestTest_NoRequests()
        {
            //Test to ensure that when an agent gets no requests - they will be removed from the list of agents to advertise for so that
            //advertsising agent may progress to advertise for the next house agent

            //Initialising house agents 
            var house0 = new HouseAgent(new SelfishBehaviour(), 0);
            house0.Name = "house0";
            house0.AllocatedSlots = new List<int>() { 1, 2, 3, 4 };
            house0.RequestedSlots = new List<int>() { 6, 2, 3, 9 };

            var house1 = new HouseAgent(new SelfishBehaviour(), 1);
            house1.Name = "house1";

            //Adding houses to list of house agents in datastore
            DataStore dataStore = DataStore.Instance;

            dataStore.HouseAgents.Clear();
            dataStore.HouseAgents.Add(house0);
            dataStore.HouseAgents.Add(house1);

            //Setting up advertising agent
            var advert = new AdvertisingAgent(3);
            advert.CurrentAdvertisingHouse = "house0";

            advert.ListHouseTimeSlots("house0", new List<string>() { "1", "4" });

            string message = advert.RequestHandler();

            //Advertising agent should be removed from list of agents to advertise for since they will not be able to make another interaction this round
            Assert.AreEqual(0, advert.AdvertisedSlots.Count);
        }
    }
}