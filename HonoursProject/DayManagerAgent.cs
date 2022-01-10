using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using ActressMas;

namespace HonoursProject.behaviours
{
    class DayManagerAgent : Agent
    {
        private int _numOfDays; //Increment for how many days have passed in the model
        private int _maxDays;
        private List<int> availableTimeSlots = new List<int>();
        private int _numberOfEvolvingAgents;
        private DataStore _dataStore = DataStore.Instance;
        private List<string> _readyAgents = new List<string>();

        public DayManagerAgent(int numberOfEvolvingAgents, int days)
        {
            this._numberOfEvolvingAgents = numberOfEvolvingAgents;
            this._maxDays = days;
        }

        public override void Setup()
        {
            CreateAvailableSlots();
            AllocateSlots();

            /*Thread.Sleep(20);
            int curve = 0;
            foreach (HouseAgent agent in _dataStore.HouseAgents)
            {
                //Could try moving this back in to environment memory
                agent.RequestingSlotHandler(_dataStore.BucketedDemandCurve[curve], _dataStore.TotalDemand[curve]);
                agent.RandomSlotAllocationHandler();
                curve++;
                if (curve >= _dataStore.BucketedDemandCurve.Count)
                {
                    curve = 0;
                }
            }

            Broadcast("allocate");*/
        }

        private void AllocateSlots()
        {
            Thread.Sleep(20);
            int curve = 0;
            foreach (HouseAgent agent in _dataStore.HouseAgents)
            {
                //Could try moving this back in to environment memory
                agent.RequestingSlotHandler(_dataStore.BucketedDemandCurve[curve], _dataStore.TotalDemand[curve]);
                agent.RandomSlotAllocationHandler();
                curve++;
                if (curve >= _dataStore.BucketedDemandCurve.Count)
                {
                    curve = 0;
                }
            }

            Broadcast("allocate");
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);
                switch (action)
                {
                    case "readyNextDay":
                        this._readyAgents.Add(message.Sender);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override void ActDefault()
        {
            if (_dataStore.HouseAgents.Count == 0)
            {
                Stop();
            } //If there are no agents in the model - leave the model

            //If all agents are ready to proceed to next day - then agents will be activated for next day
            if (this._readyAgents.Count == _dataStore.HouseAgents.Count)
            {
                if (this._numOfDays < this._maxDays-1)
                {
                    Thread.Sleep(20);
                    this._numOfDays++;
                    this._readyAgents.Clear();

                    EndOfDaySocialLearning();

                    Console.WriteLine($"***************** END OF DAY {this._numOfDays} *********************");
                    Thread.Sleep(200);

                    AllocateSlots();
                                    
                    Send("advertiser", "newDay");
                }
                else
                {
                    Console.WriteLine($"***************** END OF DAY {this._numOfDays+1} (MAXIMUM) *********************");
                    Stop();
                }
            }
        }

        //Function that will create available time slots for agents for the day
        private void CreateAvailableSlots()
        {
            int populationSize = Environment.Memory["NoOfAgents"];
            int uniqueTimeSlots = Environment.Memory["UniqueTimeSlots"];

            int requiredTimeSlots = populationSize * uniqueTimeSlots;
            List<int> possibleTimeSlots = new List<int>();

            while (availableTimeSlots.Count < requiredTimeSlots)
            {
                for (int timeSlot = 1; timeSlot <= uniqueTimeSlots; timeSlot++)
                {
                    possibleTimeSlots.Add(timeSlot);
                }

                while (possibleTimeSlots.Count > 0)
                {
                    if (availableTimeSlots.Count < requiredTimeSlots)
                    {
                        //Random rand = Environment.Memory["EnvRandom"];
                        Random rand = _dataStore.EnvironmentRandom;
                        int selector = rand.Next(possibleTimeSlots.Count);
                        int timeSlot = possibleTimeSlots[selector];
                        availableTimeSlots.Add(timeSlot);
                        possibleTimeSlots.Remove(selector);
                    }
                    else
                    {
                        possibleTimeSlots.Clear();
                        break;
                    }
                }
            }

            _dataStore.AvailableSlots = availableTimeSlots;
            //Environment.Memory.Add("AvailableSlots", availableTimeSlots);
        }

        //Function that will handle the social learning of agents after all exchanges have been made for a day
        private void EndOfDaySocialLearning()
        {
            List<HouseAgent> houseAgents = _dataStore.HouseAgents;
            //Random rand = Environment.Memory["EnvRandom"];
            Random rand = _dataStore.EnvironmentRandom;

            //If list is empty, do nothing and exit function
            if (houseAgents.Count == 0) { return; }

            int numberOfTimeSlots = houseAgents[0].GetNumberOfTimeSlots; //Every agent will have same number of time slots - so using first agent in list to retrieve this value
            int numberOfAgents = houseAgents.Count;

            List<Tuple<IBehaviour, double>> previousPerformances = Enumerable.Repeat(new Tuple<IBehaviour, double>(null, 0), houseAgents.Count).ToList(); //Filling list of previous agent performances

            foreach (HouseAgent agent in houseAgents)
            {
                IBehaviour type = agent.Behaviour;
                double satisfaction = agent.CalculateSatisfaction(null);

                previousPerformances[agent.GetID] = new Tuple<IBehaviour, double>(type, satisfaction);
            }

            //Copying agents to store all agents that have not been selected for social learning
            List<HouseAgent> unselectedAgents = new List<HouseAgent>(houseAgents);

            for (int i = 0; i < this._numberOfEvolvingAgents; i++)
            {
                int observedPerformance = rand.Next(houseAgents.Count);

                HouseAgent learningAgent = unselectedAgents[rand.Next(unselectedAgents.Count)];

                //Method to ensure that the learning agent selects an agent to learn from that is not itself
                while (learningAgent.GetID == observedPerformance)
                {
                    observedPerformance = rand.Next(houseAgents.Count);
                }

                double learningAgentSatisfaction = learningAgent.CalculateSatisfaction(null);
                double observedAgentSatisfaction = previousPerformances[observedPerformance].Item2;
                if (Math.Round(learningAgentSatisfaction * numberOfTimeSlots) <
                    Math.Round(observedAgentSatisfaction * numberOfTimeSlots))
                {
                    double difference = observedAgentSatisfaction - learningAgentSatisfaction;
                    double threshold = rand.NextDouble();

                    //Agent will swap strategy if behaviour is not the same as how the agent is currently behaving and if the difference is greater than the threshold
                    if (difference > threshold && learningAgent.Behaviour != previousPerformances[observedPerformance].Item1)
                    {
                        learningAgent.Behaviour.SwitchStrategy(learningAgent);
                    }
                }

                unselectedAgents.Remove(learningAgent);
            }
        }
    }
}
