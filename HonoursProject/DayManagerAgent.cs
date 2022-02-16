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
            int uniqueTimeSlots = Environment.Memory["UniqueTimeSlots"];
            foreach (HouseAgent agent in _dataStore.HouseAgents)
            {
                agent.RequestedSlots.Clear();
                agent.AllocatedSlots.Clear();

                agent.RequestedSlotAllocationHandler(uniqueTimeSlots);
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

        //! Function that will handle the progression of days in the model.
        /*!
          This function will execute when the day manager agent receives a "prepareForNextDay" message which comes from the advertising agent.
          Upon receiving this message, the day manager will increment the day counter, create and allocate the slots to house agents for the next day as well as run the calculations that are required
          at the start and end of each day in the model.
         */
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
         Day manager will use this in the case where no house agents are present in the model. If there are no house agents, then the day manager will leave the environment.
         */
        public override void ActDefault()
        {
            if (_dataStore.HouseAgents.Count == 0)
            {
                Stop(); //If there are no agents in the model - leave the model
            } 
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
        }

        //! End of day social learning function.
        /*!
         Function that will handle the social learning of agents after all exchanges have been made for a day
         */
        private void EndOfDaySocialLearning()
        {
            //If list is empty, do nothing and exit function
            if (_dataStore.HouseAgents.Count == 0) { return; }

            int numberOfTimeSlots = _dataStore.HouseAgents[0].GetNumberOfTimeSlots; //Every agent will have same number of time slots - so using first agent in list to retrieve this value
            int numberOfAgents = _dataStore.HouseAgents.Count;

            List<Tuple<int, IBehaviour, double>> previousPerformances = Enumerable.Repeat(new Tuple<int, IBehaviour, double>(0, null, 0), _dataStore.HouseAgents.Count).ToList();

            List<int> unselectedAgents = new List<int>();   //List of agent ID's that have not been selected for learning

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
                }
                unselectedAgents.Remove(agent.GetID);
            }
        }
    }
}
