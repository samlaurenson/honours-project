using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using HonoursProject.behaviours;

namespace HonoursProject
{
    class DataStore
    {
        private static DataStore _instance;
        private List<HouseAgent> _houseAgents = new List<HouseAgent>();
        private Dictionary<int, List<double>> _endOfDaySatisfactions = new Dictionary<int, List<double>>(); //int -> day number, list of doubles -> satisfactions

        private List<int> _availableSlots = new List<int>();
        private Random _random;

        private List<List<double>> bucketedDemandCurves;
        private List<double> totalDemandValues;

        private DataStore() {}

        public static DataStore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataStore();
                }

                return _instance;
            }
        }

        public List<HouseAgent> HouseAgents
        {
            get { return _houseAgents; }
            set { _houseAgents = value; }
        }

        public List<int> AvailableSlots
        {
            get { return _availableSlots; }
            set { _availableSlots = value; }
        }

        public Random EnvironmentRandom
        {
            get { return _random; }
            set { _random = value; }
        }

        public List<double> TotalDemand
        {
            get { return totalDemandValues; }
            set { totalDemandValues = value; }
        }

        public List<List<double>> BucketedDemandCurve
        {
            get { return bucketedDemandCurves; }
            set { bucketedDemandCurves = value; }
        }

        public Dictionary<int, List<double>> EndOfDaySatisfactoin
        {
            get { return _endOfDaySatisfactions; }
            set { _endOfDaySatisfactions = value; }
        }

        public void CalculateEndOfDaySatisfactions(int day)
        {
            List<double> satisfactions = new List<double>();

            satisfactions.Add(averageAgentSatisfaction());
            satisfactions.Add(optimumAgentSatisfaction());

            //Adding average satisfaction of each agent type to list
            calculateSatisfactionForAgentTypes(satisfactions);

            //Getting the average variance for each agent type
            endOfDaySatisfactionStandardDeviation(satisfactions);

            _endOfDaySatisfactions.Add(day, satisfactions);
        }

        private double averageAgentSatisfaction()
        {
            List<double> agentSatisfactions = new List<double>();

            //Getting satisfaction for each agent in order to get the average satisfaction
            foreach (var agent in HouseAgents)
            {
                agentSatisfactions.Add(agent.CalculateSatisfaction(null));
            }

            return agentSatisfactions.Average();
        }

        //Function will calculate the optimum average satisfaction that could be obtained for all agents based on
        //requests and allocations
        private double optimumAgentSatisfaction()
        {
            List<int> allRequestedSlots = new List<int>();
            List<int> allAllocatedSlots = new List<int>();

            foreach (var agent in HouseAgents)
            {
                agent.RequestedSlots.ForEach(x => allRequestedSlots.Add(x));
                agent.AllocatedSlots.ForEach(x => allAllocatedSlots.Add(x));
            }

            //Stores number of slots that could potentially be fulfilled with perfect trading
            double satisfiedSlots = 0;

            //Storing total number of slot requests by all agents
            double totalSlots = allRequestedSlots.Count;

            foreach (int slot in allRequestedSlots)
            {
                if(allAllocatedSlots.Contains(slot))
                {
                    //For each request - if request has been allocated to any agent then increase number of satisfied slots
                    satisfiedSlots++;

                    //Remove slot from the list of all allocated slots so not slots can be allocated more than once
                    allAllocatedSlots.Remove(slot);
                }
            }

            return satisfiedSlots / totalSlots;
        }

        //Function that will calculate the average agent satisfaction for each agent type
        private void calculateSatisfactionForAgentTypes(List<double> satisfactions)
        {
            //var agentTypes = HouseAgents.GroupBy(agent => agent.Behaviour).Select(group => group.ToList()).ToList();
            var agentTypes = HouseAgents.GroupBy(agent => agent.Behaviour.GetType()).ToList();

            foreach (var typeList in agentTypes)
            {
                double satisfaction = 0;
                foreach(var agent in typeList)
                {
                    satisfaction += agent.CalculateSatisfaction(null);
                }
                satisfactions.Add(satisfaction/typeList.Count());
            }
        }

        private void endOfDaySatisfactionStandardDeviation(List<double> satisfactions)
        {

        }
    }
}
