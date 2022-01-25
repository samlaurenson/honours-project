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
            dataStore.EnvironmentRandom = new Random();
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
            //Testing that requests are removing the requested slot from the list of advertised slots -- ensures that the slot cannot be requested more than once

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

            advert.Requests.Add(new Tuple<string, int, int>("house1", 1, 6));   //House 1 sent request for slot 1 that House 0 has in exchange for slot 6
            
            string message = advert.RequestHandler();

            //Should only have 1 slot left after requested slot was removed from the advertised slot list
            Assert.AreEqual(1, advert.AdvertisedSlots["house0"].Count);
        }
    }
}