﻿using System;
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

        private Dictionary<int, List<List<List<double>>>> _simulations = new Dictionary<int, List<List<List<double>>>>(); //int -> simulation number, list is the data for models running
                                                                                                                                                //the different number of evolving agents
        private List<List<double>> _endOfDaySatisfactions = new List<List<double>>(); //int -> day number, list of doubles -> satisfactions

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

        public List<List<double>> EndOfDaySatisfaction
        {
            get { return _endOfDaySatisfactions; }
            set { _endOfDaySatisfactions = value; }
        }

        public Dictionary<int, List<List<List<double>>>> SimulationData
        {
            get { return _simulations; }
            set { _simulations = value; }
        }

        public void CalculateEndOfDaySatisfactions(int day)
        {
            //_endOfDaySatisfactions.Clear();
            List<double> satisfactions = new List<double>();

            satisfactions.Add(averageAgentSatisfaction());
            satisfactions.Add(optimumAgentSatisfaction());

            var agentTypes = HouseAgents.GroupBy(agent => agent.Behaviour.GetType()).ToList();

            //Calculating average satisfaction and variance for each agent type
            foreach (var type in agentTypes)
            {
                //Adding average satisfaction of each agent type to list
                satisfactions.Add(calculateSatisfactionForAgentTypes(type.ToList()));

                //Getting the average variance for each agent type
                satisfactions.Add(endOfDaySatisfactionStandardDeviation(type.ToList()));
            }

            //First 2 elements in list will be the average and optimum agent satisfactions
            //After the first 2 elements, then elements will be the average satisfaction for agent type followed by average variance for agent type
            //And will follow this pattern for each agent type
            //e.g. 0 -> average agent satisfaction, 1 -> optimum agent satisfaction,
            // 2 -> Selfish agent satisfaction, 3 -> selfish agent satisfaction variance,
            // 4 -> Social agent satisfaction, 5 -> social agent satisfaction variance, and so on...
            _endOfDaySatisfactions.Add(satisfactions);
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
        private double calculateSatisfactionForAgentTypes(List<HouseAgent> agents)
        {
            double satisfaction = 0.0;

            foreach (var agent in agents)
            {
                satisfaction += agent.CalculateSatisfaction(null);
            }

            return satisfaction;
        }

        private double endOfDaySatisfactionStandardDeviation(List<HouseAgent> agents)
        {
            double sumDiffsSquared = 0.0;
            int groupSize = 0;
            double averageSatisfaction = calculateSatisfactionForAgentTypes(agents);

            foreach (var agent in agents)
            {
                double diff = agent.CalculateSatisfaction(null) - averageSatisfaction;
                diff *= diff;
                sumDiffsSquared += diff;
                groupSize++;
            }

            double populationVariance = sumDiffsSquared / (double)groupSize;
            return Math.Sqrt(populationVariance);
        }
    }
}
