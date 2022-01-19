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
    }
}