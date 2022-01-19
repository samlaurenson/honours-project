using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ActressMas;
using HonoursProject.behaviours;

namespace HonoursProject
{
    //! Class for the House Agent.
    /*!
     House agents are the agents that will be interacting with other house agents to complete exchanges.
     */
    public class HouseAgent : Agent
    {
        private IBehaviour _agentBehaviour; /*!< variable that stores the behaviour strategy that the agent will follow. */
        private DataStore _dataStore = DataStore.Instance; /*!< Reference to the DataStore instance. */
        private int _id; /*!< variable that is the ID of the house agent. */
        private int _satisfaction;
        private bool _madeInteraction; /*!< variable that is used as a flag to signal whether the agent has made an interaction this round or not. */
        private bool _useSocialCapital; /*!< variable that will be used to determine whether the model will use social capital or not. */
        private List<int> _allocatedSlots = new List<int>(); /*!< List of time slots the agent has been allocated. */
        private List<int> _requestedSlots = new List<int>(); /*!< List of requested slots the agent desires. */
        private Dictionary<string, int> _favoursOwed = new Dictionary<string, int>(); /*!< Dictionary of other agent names and the number of times this agent owes another agent a favour */
        private Dictionary<string, int> _favoursGiven = new Dictionary<string, int>(); /*!< Dictionary of other agent names and the number of times this agent has given another agent a favour */

        private static int curve = 0;
        private static int numberOfTimeSlotsWanted = 4; /*!< The number of time slots each agent will have. */

        private List<double> _agentFlexibility; /*!< List of values that determines the flexibility of the agent - used to calculate the agent's satisfaction. */
        //Using 2 entries for agent flexibility so that unit tests could be made to fully test the satisfaction calculation function

        //! Constructor for the house agent.
        /*!
         \param behaviour The behaviour strategy that the agent will follow.
         \param id The id of the house agent.
         */
        public HouseAgent(IBehaviour behaviour, int id)
        {
            this._agentBehaviour = behaviour;
            this._id = id;
        }

        //! Getter and setter for agent behaviour.
        public IBehaviour Behaviour
        {
            get { return this._agentBehaviour; }
            set { this._agentBehaviour = value; }
        }

        //! Getter and setter for made interaction flag.
        public bool MadeInteraction
        {
            get { return _madeInteraction; }
            set { _madeInteraction = value; }
        }

        //! Getter for house agent id.
        public int GetID
        {
            get { return this._id; }
        }

        //! Getter and setter for social capital flag.
        public bool SocialCapital
        {
            get { return this._useSocialCapital; }
            set { this._useSocialCapital = value; }
        }

        //! Getter and setter for favours owed dictionary.
        public Dictionary<string, int> FavoursOwed
        {
            get { return this._favoursOwed; }
            set { this._favoursOwed = value; }
        }

        //! Getter and setter for favours given dictionary.
        public Dictionary<string, int> FavoursGiven
        {
            get { return this._favoursGiven; }
            set { this._favoursGiven = value; }
        }

        //! Getter and setter for allocated slots list.
        public List<int> AllocatedSlots
        {
            get {return this._allocatedSlots;}
            set { this._allocatedSlots = value; }
        }

        //! Getter and setter for requested slots list.
        public List<int> RequestedSlots
        {
            get { return this._requestedSlots; }
            set { this._requestedSlots = value; }
        }

        //! Getter and setter for agent flexibility list.
        public List<double> AgentFlexibility
        {
            get { return this._agentFlexibility; }
            set { this._agentFlexibility = value; }
        }

        //! Getter for number of time slots per agent.
        public int GetNumberOfTimeSlots
        {
            get { return numberOfTimeSlotsWanted; }
        }

        //! Setup function
        /*!
         Will execute when environment starts.
         */
        public override void Setup()
        {
            //Thread.Sleep(50); 
            //_dataStore.HouseAgents.Add(this);
        }

        //! Act function.
        /*!
         Will run whenever agent receives a message.
         Accepted messages include - "allocate", "notify", "undoInteraction", "sendRequest", "acceptRequest", "prepareForNextDay", "Stop"
         \param message The message that the agent has received.
         */
        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "allocate":
                        //initial allocation of slots for the day
                        Console.WriteLine(Name + " Satisfaction: " + CalculateSatisfaction(null));
                        this._madeInteraction = false;

                        string advertiseSlots = ListUnwantedSlots();

                        //If no slots unwanted - then don't send message to advertiser
                        if (string.IsNullOrWhiteSpace(advertiseSlots))
                        {
                            break;
                        }

                        Send("advertiser", $"list {advertiseSlots}");
                        break;
                    case "notify":
                        //advertising agent lets house agents know when there is a slot available to exchange

                        //Thread.Sleep(50);
                        //Only do this if madeInteraction is true - if false then break
                        if (this._madeInteraction)
                        {
                            break;
                        }
                        else
                        {
                            string advertisedHouse = parameters[0];


                            /*if (_dataStore.HouseAgents[_dataStore.HouseAgents.FindIndex(agent => agent.Name == advertisedHouse)]
                                .MadeInteraction)
                            {
                                return;
                            }*/

                            List<int> advertisingAgentSlots = new List<int>();
                            for (int i = 1; i < parameters.Count; i++)
                            {
                                advertisingAgentSlots.Add(Int32.Parse(parameters[i]));
                            }

                            int? slotToExchange = null;
                            //Getting an unwanted slot to swap for a desired slot
                            for (int i = 0; i < AllocatedSlots.Count; i++)
                            {
                                if (!RequestedSlots.Contains(AllocatedSlots[i]))
                                {
                                    slotToExchange = AllocatedSlots[i];
                                }
                            }

                            //Getting a desired slot
                            //If the agent advertising their slot has a slot that is in this agents requested slots - then this will be the desired slot
                            foreach (int requestSlot in RequestedSlots)
                            {
                                if (advertisingAgentSlots.Contains(requestSlot) && slotToExchange != null)
                                {
                                    Send("advertiser", $"request {requestSlot} {slotToExchange}");
                                    this._madeInteraction = true;
                                    break;
                                }
                            }
                        }

                        break;
                    case "undoInteraction":
                        this._madeInteraction = false;
                        break;
                    case "sendRequest":
                        //gets message from advertising agent when another agent requests a slot that this agent has
                        //Message parameters : P0 -> requesting agent name (string), P1 -> slot to be exchanged for desired slot (int), P2 -> slot the requesting agent wants from this agent (int)

                        string requestingAgentName = parameters[0];
                        int requestingAgentSlot = Int32.Parse(parameters[1]);
                        int requestingAgentDesiredSlot = Int32.Parse(parameters[2]);
                        //Thread.Sleep(50);

                        //Only do this if madeInteraction is true - if false then break
                        if (this._madeInteraction)
                        {
                            Send("advertiser", $"requestUnsuccessful {requestingAgentDesiredSlot}");
                            break;
                        }

                        this._madeInteraction = true;

                        bool decision = Behaviour.ConsiderRequest(this, requestingAgentName, requestingAgentSlot, requestingAgentDesiredSlot);

                        if (decision)
                        {
                            //Exchange was successful -- agent will replace the slot they had with the requesting agents slot
                            this.AllocatedSlots.Remove(requestingAgentDesiredSlot);
                            this.AllocatedSlots.Add(requestingAgentSlot);

                            //If agent is a social agent - then remember that they have given the requesting agent a favour
                            if (_agentBehaviour is SocialBehaviour)
                            {
                                if (!FavoursGiven.ContainsKey(message.Sender))
                                {
                                    FavoursGiven.Add(message.Sender, 1);
                                }
                                else
                                {
                                    FavoursGiven[message.Sender]++;
                                }
                            }

                            //Sends message to the requesting agent with the slot they have (and need to replace) with their desired slot
                            Send(requestingAgentName, $"acceptRequest {requestingAgentSlot} {requestingAgentDesiredSlot}");
                        }
                        else
                        {
                            Send("advertiser", $"requestUnsuccessful {requestingAgentDesiredSlot}");
                        }

                        break;
                    case "acceptRequest":
                        //Exchange was successful -- so will need to replace current slot with the desired slot
                        //Message parameters : P0 -> slot this agent currently has, P1 -> agents desired slot
                        Console.WriteLine(Name + " had a successful trade with " + message.Sender);
                        int currentSlot = Int32.Parse(parameters[0]);
                        int desiredSlot = Int32.Parse(parameters[1]);
                        HandleAcceptedRequest(currentSlot, desiredSlot, message.Sender);
                        break;
                    case "prepareForNextDay":
                        //This message will come from the advertising agent who will send this after all the exchange rounds have been completed
                        //Will need to calculate and store end of day satisfaction and then notify day manager that agents are ready to proceed

                        this._madeInteraction = false;
                        Send("daymanager", "readyNextDay");
                        break;
                    case "Stop":
                        Stop();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //! Function to handle case where exchange was successful and agent receives the slot they wanted.
        /*!
         Function will replace the current slot the agent has in their allocated slot list with their desired slot provided by the other agent in the exchange.
         \param currentSlot The slot the agent has and wants to exchange.
         \param desiredSlot The slot the agent wants from another agent.
         \param agentWithDesiredSlot The name of the agent who has the desired slot.
         */
        public void HandleAcceptedRequest(int currentSlot, int desiredSlot, string agentWithDesiredSlot)
        {
            //If requesting agent is a social agent, then remember that they owe a favour to the agent that accepted the request
            if (_agentBehaviour is SocialBehaviour)
            {
                if (!FavoursOwed.ContainsKey(agentWithDesiredSlot))
                {
                    FavoursOwed.Add(agentWithDesiredSlot, 1);
                }
                else
                {
                    FavoursOwed[agentWithDesiredSlot]++;
                }
            }

            this.AllocatedSlots.Remove(currentSlot);
            this.AllocatedSlots.Add(desiredSlot);
        }

        //! Function that will create a list of unwanted slots that the house will send to the advertiser
        /*!
         \return String of unwanted time slots to send to advertiser.
         */
        public string ListUnwantedSlots()
        {
            //Listing slots - if a slot in the allocated list is not in the requested list - then this is considered unwanted and will be listed
            List<int> slotsToList = new List<int>();
            foreach (int slot in AllocatedSlots)
            {
                if (!RequestedSlots.Contains(slot))
                {
                    slotsToList.Add(slot);
                }
            }

            if (slotsToList.Count == 0)
            {
                return "";
            }

            return string.Join(" ", slotsToList.ToArray()); //Turning slots to advertise in to string format so they can be sent to advertiser through message passing
        }

        //! Function that will be used to calculate the satisfaction of house agents.
        /*!
         \param timeSlots Calculates satisfaction for the given list of time slots. If null, will use the list of allocated slots for this agent.
         */
        public double CalculateSatisfaction(List<int> timeSlots)
        {
            //If there is no input for function, then the slots the agent has will be used for calculating the satisfaction
            if (timeSlots == null)
            {
                timeSlots = this._allocatedSlots;
            }

            //Precaution in case both lists are empty. This should not happen but this has been put here to prevent the program crashing if this case does happen.
            if (this._requestedSlots.Count == 0 && timeSlots.Count == 0)
            {
                return 0;
            }

            List<int> tempRequestedTimeSlots = new List<int>(this._requestedSlots);
            List<int> tempAllocatedTimeSlots = new List<int>(timeSlots);

            //Counting the number of allocated time slots that match the agents requested time slots
            double totalSatisfaction = 0;


            for (int i = 0; i < _agentFlexibility.Count; i++)
            {
                List<int> tempAllocatedTimeSlotsCopy = new List<int>(tempAllocatedTimeSlots);
                foreach (var allocatedSlot in tempAllocatedTimeSlotsCopy)
                {
                    if (tempRequestedTimeSlots.Count == 0)
                    {
                        return totalSatisfaction / numberOfTimeSlotsWanted;
                    }

                    if (i == 0)
                    {
                        if (tempRequestedTimeSlots.Contains(allocatedSlot))
                        {
                            tempRequestedTimeSlots.Remove(allocatedSlot);
                            tempAllocatedTimeSlots.Remove(allocatedSlot);
                            totalSatisfaction += _agentFlexibility[i];
                        }
                    }
                    else
                    {
                        foreach (var requestedSlot in tempRequestedTimeSlots)
                        {
                            int temp = requestedSlot + i;
                            if (temp > 24)
                            {
                                temp -= 24;
                            }

                            int temp2 = requestedSlot - i;
                            if (temp2 < 1)
                            {
                                temp2 += 24;
                            }

                            if (allocatedSlot == temp)
                            {
                                tempRequestedTimeSlots.Remove(requestedSlot);
                                tempAllocatedTimeSlots.Remove(allocatedSlot);
                                totalSatisfaction += _agentFlexibility[i];
                                break;
                            } else if (allocatedSlot == temp2)
                            {
                                tempRequestedTimeSlots.Remove(requestedSlot);
                                tempAllocatedTimeSlots.Remove(allocatedSlot);
                                totalSatisfaction += _agentFlexibility[i];
                                break;
                            }
                        }
                    }
                }
            }
            return totalSatisfaction / numberOfTimeSlotsWanted;
        }

        //! Function that will determine the slots the agent will request.
        /*!
         Determines the slots the agent will request based on the demand curve.
        \param demandCurve The list of demand curve values.
        \param totalDemand Value of demand based on demand curve.
         */
        public void RequestingSlotHandler(List<double> demandCurve, double totalDemand)
        {
            if (_requestedSlots.Count > 0)
            {
                _requestedSlots.Clear();
            }

            for (int i = 1; i <= numberOfTimeSlotsWanted; i++)
            {
                Random rand = _dataStore.EnvironmentRandom;
                // Selects a time slot based on the demand curve
                int wheelSelector = rand.Next((int)(totalDemand * 10));
                int wheelCalculator = 0;
                int timeSlot = 0;
                while (wheelCalculator < wheelSelector)
                {
                    wheelCalculator = wheelCalculator + ((int)(demandCurve[timeSlot] * 10));
                    timeSlot++;
                }

                // Ensures all requested time slots are unique
                if (_requestedSlots.Contains(timeSlot))
                {
                    i--;
                }
                else
                {
                    _requestedSlots.Add(timeSlot);
                }
            }
        }

        //! Function that will randomly allocate slots for this agent.
        public void RandomSlotAllocationHandler()
        {
            List<int> availableTimeSlots = _dataStore.AvailableSlots;
            for (int requestedTimeSlot = 1; requestedTimeSlot <= _requestedSlots.Count; requestedTimeSlot++)
            {
                if (availableTimeSlots.Count > 0)
                {
                    Random rand = DataStore.Instance.EnvironmentRandom;
                    int selector = rand.Next(availableTimeSlots.Count);
                    int timeSlot = availableTimeSlots[selector];
                    _allocatedSlots.Add(timeSlot);
                }
                else
                {
                    Console.WriteLine("No time slots available");
                }
            }
        }
    }
}
