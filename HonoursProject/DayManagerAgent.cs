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
    public class DayManagerAgent : Agent
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

        //! Getter for current day in the model.
        public int CurrentDay
        {
            get { return _numOfDays; }
        }

        //! Setup function
        /*!
         Will execute when environment starts.
         */
        public override void Setup()
        {
            CreateSlots();
            AllocateSlots();

            _dataStore.addStartOfDaySatisfactions(); //getting optimal and random allocation values
            Broadcast("allocate");
        }

        //! Create available slots function.
        /*!
         Function that will create available time slots for agents for the day.
         Will fill the capacity for each unique time slot. 
         So, if there are 24 unique time slots with a maximum capacity of 16 slots, then 16 instances of time slots 1 through 24 will be created and available for allocation.
         */
        private void CreateSlots()
        {
            availableTimeSlots.Clear();

            int uniqueTimeSlots = Environment.Memory["UniqueTimeSlots"];
            int maxCapacity = Environment.Memory["MaxSlotCapacity"];

            for (int timeslot = 1; timeslot <= uniqueTimeSlots; timeslot++)
            {
                for (int unit = 1; unit <= maxCapacity; unit++)
                {
                    availableTimeSlots.Add(timeslot);
                }
            }

            _dataStore.AvailableSlots = availableTimeSlots;
        }

        //! Function to allocate slots to house agents.
        /*!
         Function to allocate slots to house agents at the beginning of each day.
         */
        private void AllocateSlots()
        {
            foreach (HouseAgent agent in _dataStore.HouseAgents)
            {
                agent.RequestedSlots.Clear();
                agent.AllocatedSlots.Clear();

                agent.RequestedSlotAllocationHandler();
                agent.RandomSlotAllocationHandler();
            }
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
                    case "prepareForNextDay":
                        NextDayHandler();
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

        private void NextDayHandler()
        {
            if (this._numOfDays < this._maxDays - 1)
            {
                EndOfDayManager();

                CreateSlots();
                AllocateSlots(); //Allocating slots for new day

                _dataStore.addStartOfDaySatisfactions(); //Calculating satisfactions of agents at start of new day

                Send("advertiser", "newDay");
                Broadcast("allocate");
            }
            else
            {
                //EndOfDayManager();
                _dataStore.CalculateEndOfDaySatisfactions(this._numOfDays);
                //Console.WriteLine($"***************** END OF DAY {this._numOfDays+1} (MAXIMUM) *********************");
                Broadcast("Stop");
                Stop();
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

            /*//If all agents are ready to proceed to next day - then agents will be activated for next day
            if (this._readyAgents.Count == _dataStore.HouseAgents.Count)
            {
                if (this._numOfDays < this._maxDays-1)
                {
                    EndOfDayManager();

                    CreateSlots();
                    AllocateSlots(); //Allocating slots for new day

                    _dataStore.addStartOfDaySatisfactions(); //Calculating satisfactions of agents at start of new day

                    Send("advertiser", "newDay");
                    Broadcast("allocate");
                }
                else
                {
                    //EndOfDayManager();
                    _dataStore.CalculateEndOfDaySatisfactions(this._numOfDays);
                    //Console.WriteLine($"***************** END OF DAY {this._numOfDays+1} (MAXIMUM) *********************");
                    Broadcast("Stop");
                    Stop();
                }
            }*/
        }

        //! Function that will run functions that are to be executed at the end of a day and increment day counter.
        /*!
          The AllocatedSlots function is not included in this handler function as the AllocatedSlots function requires randomness, which is difficult to unit test, and also
          requires an ActressMAS environment to be running since the AllocatedSlots function will broadcast messages to agents, if there is no environment running then this would cause an issue.
         */
        public void EndOfDayManager()
        {
            _dataStore.CalculateEndOfDaySatisfactions(this._numOfDays);
            this._numOfDays++;
            this._readyAgents.Clear();

            EndOfDaySocialLearning();

            //Console.WriteLine($"Number social AFTER LEARNING: {_dataStore.HouseAgents.Where(agent => agent.Behaviour is SocialBehaviour).Count()}");
            //Console.WriteLine($"Number selfish AFTER LEARNING: {_dataStore.HouseAgents.Where(agent => agent.Behaviour is SelfishBehaviour).Count()}");


            //Console.WriteLine($"***************** END OF DAY {this._numOfDays} *********************");
        }

        private void EndOfDaySocialLearning2()
        {
            if (_dataStore.HouseAgents.Count == 0) { return; }

            List<HouseAgent> unselected = new List<HouseAgent>(_dataStore.HouseAgents);

            for (int i = 0; i < _numberOfEvolvingAgents; i++)
            {
                int randomID = _dataStore.EnvironmentRandom.Next(unselected.Count);
                HouseAgent learningAgent = unselected[randomID];

                unselected.Remove(learningAgent);

                int randomObservedID = _dataStore.EnvironmentRandom.Next(unselected.Count);

                /*while (randomObservedID == randomID)
                {
                    randomObservedID = _dataStore.EnvironmentRandom.Next(unselected.Count);
                }*/

                HouseAgent observedAgent = unselected[randomObservedID];

                while (observedAgent.GetID == learningAgent.GetID)
                {
                    randomObservedID = _dataStore.EnvironmentRandom.Next(unselected.Count);
                    observedAgent = unselected[randomObservedID];
                }

                learningAgent.learn(observedAgent);
            }
        }

        //! End of day social learning function.
        /*!
         Function that will handle the social learning of agents after all exchanges have been made for a day
         */
        private void EndOfDaySocialLearning()
        {
            List<HouseAgent> houseAgents = _dataStore.HouseAgents;
            //Random rand = Environment.Memory["EnvRandom"];
            //Random rand = _dataStore.EnvironmentRandom;

            //If list is empty, do nothing and exit function
            if (_dataStore.HouseAgents.Count == 0) { return; }

            int numberOfTimeSlots = _dataStore.HouseAgents[0].GetNumberOfTimeSlots; //Every agent will have same number of time slots - so using first agent in list to retrieve this value
            int numberOfAgents = _dataStore.HouseAgents.Count;

            List<Tuple<int, IBehaviour, double>> previousPerformances = Enumerable.Repeat(new Tuple<int, IBehaviour, double>(0, null, 0), _dataStore.HouseAgents.Count).ToList();

            List<int> unselectedAgents = new List<int>();

            foreach (HouseAgent agent in _dataStore.HouseAgents)
            {
                double currentSatisfaction =
                    (int)(Math.Round(agent.CalculateSatisfaction(null) * agent.GetNumberOfTimeSlots));

                previousPerformances[agent.GetID] =
                    new Tuple<int, IBehaviour, double>(agent.GetID, agent.Behaviour, currentSatisfaction);

                unselectedAgents.Add(agent.GetID);
            }

            for (int i = 0; i < _numberOfEvolvingAgents; i++)
            {
                int randomID = _dataStore.EnvironmentRandom.Next(unselectedAgents.Count);
                HouseAgent agent = _dataStore.HouseAgents.Find(agent => agent.GetID == randomID);

                int random = _dataStore.EnvironmentRandom.Next(_dataStore.HouseAgents.Count);

                Tuple<int, IBehaviour, double> agentToCopy = previousPerformances[random];

                //Make sure the agent is not observing themselves
                while (agent.GetID == agentToCopy.Item1)
                {
                    random = _dataStore.EnvironmentRandom.Next(_dataStore.HouseAgents.Count);
                    agentToCopy = previousPerformances[random];
                }

                double agentSatisfaction = agent.CalculateSatisfaction(null);
                double observedSatisfaction = (double)agentToCopy.Item3 / numberOfTimeSlots;
                if (agentSatisfaction < observedSatisfaction)
                {
                    double difference = observedSatisfaction - agentSatisfaction;
                    double threshold = _dataStore.EnvironmentRandom.NextDouble();
                    if (difference > threshold)
                    {
                        if (agentToCopy.Item2 is SocialBehaviour)
                        {
                            agent.Behaviour = new SocialBehaviour();
                        }
                        else
                        {
                            agent.Behaviour = new SelfishBehaviour();
                        }
                    }

                    /*if (difference > threshold && !agent.Behaviour.Equals(agentToCopy.Item2))
                    {
                        agent.Behaviour.SwitchStrategy(agent);
                    }*/
                }

                unselectedAgents.Remove(agent.GetID);
            }

            /*List<Tuple<IBehaviour, double>> previousPerformances = Enumerable.Repeat(new Tuple<IBehaviour, double>(null, 0), houseAgents.Count).ToList(); //Filling list of previous agent performances

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

                //unselectedAgents.Remove(learningAgent);

                //Method to ensure that the learning agent selects an agent to learn from that is not itself
                while (learningAgent.GetID == observedPerformance)
                {
                    observedPerformance = rand.Next(houseAgents.Count);
                }

                double learningAgentSatisfaction = learningAgent.CalculateSatisfaction(null);
                double observedAgentSatisfaction = previousPerformances[observedPerformance].Item2;
                if (Math.Round(learningAgentSatisfaction * 4) <
                    Math.Round(observedAgentSatisfaction * 4))
                {
                    double difference = observedAgentSatisfaction - learningAgentSatisfaction;
                    double threshold = rand.NextDouble() * (1.0 - 0.0) + 0.0;

                    //Agent will swap strategy if behaviour is not the same as how the agent is currently behaving and if the difference is greater than the threshold
                    if (difference > threshold && !learningAgent.Behaviour.Equals(previousPerformances[observedPerformance].Item1))
                    {
                        learningAgent.Behaviour.SwitchStrategy(learningAgent);
                    }
                }

                unselectedAgents.Remove(learningAgent);
            }*/
        }
    }
}
