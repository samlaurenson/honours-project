using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using ActressMas;

namespace HonoursProject.behaviours
{
    //! DayManager Agent
    /*!
     The day manager agent is responsible for managing the progression of days in a model.
     This agent will be responsible for providing house agents with allocated and requested slots as well as progressing the day in the model.
     At the end of each day, this agent will run the function for social learning and will calculate agent satisfactions which will be stored in the DataStore class.
     */
    class DayManagerAgent : Agent
    {
        private int _numOfDays; /*!< Increment for how many days have passed in the model. */
        private int _maxDays; /*!< The number of days that the model will run for. */
        private List<int> availableTimeSlots = new List<int>(); /*!< A list of available time slots that agents can be allocated. */
        private int _numberOfEvolvingAgents; /*!< Number of agents that will evolve at the end of a day. */
        private DataStore _dataStore = DataStore.Instance; /*!< Reference to the DataStore instance. */
        private List<string> _readyAgents = new List<string>(); /*!< List of agents that are ready to progress to next day in the model. */

        //! Constructor for the day manager agent.
        /*!
         \param numberOfEvolvingAgents The number of agents that will evolve every day.
         \param days The number of days that the model will run for.
         */
        public DayManagerAgent(int numberOfEvolvingAgents, int days)
        {
            this._numberOfEvolvingAgents = numberOfEvolvingAgents;
            this._maxDays = days;
        }

        //! Setup function
        /*!
         Will execute when environment starts.
         */
        public override void Setup()
        {
            CreateAvailableSlots();
            AllocateSlots();
        }

        //! Function to allocate slots to house agents.
        /*!
         Function to allocate slots to house agents.
         Function will allocate slots to each agent in the environment based on a demand curve.
         */
        private void AllocateSlots()
        {
            Thread.Sleep(20);
            int curve = 0;
            foreach (HouseAgent agent in _dataStore.HouseAgents)
            {
                agent.RequestedSlots.Clear();
                agent.AllocatedSlots.Clear();

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

        //! Act function.
        /*!
         Will run whenever agent receives a message.
         Accepted message is the "readyNextDay" message - which will add an agents name to a list of agents that are ready to progress to next day.
         \param message The message that the agent has received.
         */
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

        //! Act default function.
        /*!
         Function will execute whenever agent has not done anything in a turn.
         This is where the day manager will check if all agents are ready to progress to the next day.
         If agents are ready to progress, then satisfactions will be calculated, social learning will take place and day counter will be incremented and agents notified that next day has started.
         */
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
                    _dataStore.CalculateEndOfDaySatisfactions(this._numOfDays);

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
                    _dataStore.CalculateEndOfDaySatisfactions(this._numOfDays);
                    Console.WriteLine($"***************** END OF DAY {this._numOfDays+1} (MAXIMUM) *********************");
                    Broadcast("Stop");
                    Stop();
                }
            }
        }

        //! Create available slots function.
        /*!
         Function that will create available time slots for agents for the day.
         */
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

        //! End of day social learning function.
        /*!
         Function that will handle the social learning of agents after all exchanges have been made for a day
         */
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
