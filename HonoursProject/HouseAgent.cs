using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private List<double> _agentFlexibility = new List<double>(); /*!< List of values that determines the flexibility of the agent - used to calculate the agent's satisfaction. */
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

                        //Don't process own advertised slots
                        if (parameters[0] == Name)
                        {
                            break;
                        }

                        //Only do this if madeInteraction is true - if not made interaction then continue
                        if (this._madeInteraction)
                        {
                            break;
                        }
                        else
                        {
                            string advertisedHouse = parameters[0];


                            List<int> advertisingAgentSlots = new List<int>();
                            for (int i = 1; i < parameters.Count; i++)
                            {
                                advertisingAgentSlots.Add(Int32.Parse(parameters[i]));
                            }

                            Tuple<int?, int?> requestInfo = null;

                            if (advertisingAgentSlots.Count > 0)
                            {
                                requestInfo = SlotToRequest(advertisingAgentSlots);
                            }

                            if (requestInfo != null)
                            {
                                this._madeInteraction = true;
                                Send("advertiser", $"request {requestInfo.Item1} {requestInfo.Item2}");
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
                        //bool decision = consider(requestingAgentName, requestingAgentSlot, requestingAgentDesiredSlot);

                        if (decision)
                        {
                            double oldSatisfaction = CalculateSatisfaction(AllocatedSlots);

                            //Exchange was successful -- agent will replace the slot they had with the requesting agents slot
                            this.AllocatedSlots.Remove(requestingAgentDesiredSlot);
                            this.AllocatedSlots.Add(requestingAgentSlot);

                            double newSatisfaction = CalculateSatisfaction(AllocatedSlots);

                            //If agent is a social agent - then remember that they have given the requesting agent a favour
                            //Favour given will only be remembered if the social agent's satisfaction is left unchanged or is a lower value after the exchange
                            if (_agentBehaviour is SocialBehaviour && newSatisfaction <= oldSatisfaction)
                            {
                                if (!_dataStore.GlobalFavoursGiven.ContainsKey(requestingAgentName))
                                {
                                    _dataStore.GlobalFavoursGiven.Add(requestingAgentName, 1);
                                }
                                else
                                {
                                    _dataStore.GlobalFavoursGiven[requestingAgentName]++;
                                }
                            }

                            //Sends message to the requesting agent with the slot they have (and need to replace) with their desired slot
                            Send(requestingAgentName, $"acceptRequest {requestingAgentSlot} {requestingAgentDesiredSlot}");
                        }

                        break;
                    case "acceptRequest":
                        //Exchange was successful -- so will need to replace current slot with the desired slot
                        //Message parameters : P0 -> slot this agent currently has, P1 -> agents desired slot
                        //Console.WriteLine(Name + " had a successful trade with " + message.Sender);
                        int currentSlot = Int32.Parse(parameters[0]);
                        int desiredSlot = Int32.Parse(parameters[1]);
                        HandleAcceptedRequest(currentSlot, desiredSlot, message.Sender);
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

        //! Function that will handle the allocation of requested slots for this agent.
        /*!
          Will randomly generate time slots that will be added to the requested time slots list and will do this until the requested slot list is full (determined by number of slots agents can have)
         */
        public void RequestedSlotAllocationHandler()
        {
            if (_requestedSlots.Count > 0)
            {
                _requestedSlots.Clear();
            }

            int uniqueTimeSlots = Environment.Memory["UniqueTimeSlots"];

            for (int i = 1; i <= numberOfTimeSlotsWanted; i++)
            {
                int timeslot = _dataStore.EnvironmentRandom.Next(uniqueTimeSlots) + 1;

                if (_requestedSlots.Contains(timeslot))
                {
                    i--;
                }
                else
                {
                    _requestedSlots.Add(timeslot);
                }
            }
        }

        //! Function that will randomly allocate slots for this agent.
        public void RandomSlotAllocationHandler()
        {
            if (_allocatedSlots.Count > 0)
            {
                _allocatedSlots.Clear();
            }
            for (int requestedTimeSlot = 1; requestedTimeSlot <= _requestedSlots.Count; requestedTimeSlot++)
            {
                if (_dataStore.AvailableSlots.Count > 0)
                {
                    int selector = _dataStore.EnvironmentRandom.Next(_dataStore.AvailableSlots.Count);
                    int timeslot = _dataStore.AvailableSlots[selector];

                    _allocatedSlots.Add(timeslot);
                    _dataStore.AvailableSlots.RemoveAt(selector);
                }
            }
        }

        //! Function that will return a List of time slots that are present in the 'extract' list but not in the 'target'
        /*!
         \param extract List of time slots - Will use this to identify slots in this list that are not in the target list
         \param target List of time slots to compare with extract
         \return List of time slots that appear in the extract list but are not present in the target list
         */
        private List<int> ExtractTimeSlots(List<int> extract, List<int> target)
        {
            List<int> localList = new List<int>(target);
            List<int> slots = new List<int>();
            foreach(int slot in extract)
            {
                if (!localList.Contains(slot))
                {
                    slots.Add(slot);
                }
                else
                {
                    localList.Remove(slot);
                }
            }

            return slots;
        }

        //! Function that will determine the slots that the agent will propose for the exchange and the slot they desire
        /*!
         \param advertisingAgentSlots The list of slots that is being advertised
         \return A Tuple which contains the desired slot (Item 1) and the slot that will be proposed in exchange (Item 2)
         */
        public Tuple<int?, int?> SlotToRequest(List<int> advertisingAgentSlots)
        {
            int? slotToRequest = null;
            int? slotToPropose = null;

            List<int> targetTimeSlots = ExtractTimeSlots(RequestedSlots, AllocatedSlots);
            List<int> unwantedSlots = ExtractTimeSlots(AllocatedSlots, RequestedSlots);

            if (targetTimeSlots.Count > 0)
            {
                foreach (int slot in advertisingAgentSlots)
                {
                    if (targetTimeSlots.Contains(slot))
                    {
                        //Does not want to exchange any slot - so will leave the function and not request any slots
                        if (unwantedSlots.Count <= 0)
                        {
                            return null;
                        }

                        slotToRequest = slot;
                        slotToPropose = unwantedSlots[_dataStore.EnvironmentRandom.Next(unwantedSlots.Count)];
                        return new Tuple<int?, int?>(slotToRequest, slotToPropose);
                    }
                }
            }

            return null;
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
            double oldSatisfaction = CalculateSatisfaction(AllocatedSlots);

            this.AllocatedSlots.Remove(currentSlot);
            this.AllocatedSlots.Add(desiredSlot);

            double newSatisfaction = CalculateSatisfaction(AllocatedSlots);

            //If requesting agent is a social agent, then remember that they owe a favour to the agent that accepted the request
            if (_agentBehaviour is SocialBehaviour && newSatisfaction > oldSatisfaction)
            {
                if (!_dataStore.GlobalFavoursOwed.ContainsKey(agentWithDesiredSlot))
                {
                    _dataStore.GlobalFavoursOwed.Add(agentWithDesiredSlot, 1);
                }
                else
                {
                    _dataStore.GlobalFavoursOwed[agentWithDesiredSlot]++;
                }
            }
        }

        //! Function that will create a list of unwanted slots that the house will send to the advertiser
        /*!
         \return String of unwanted time slots to send to advertiser.
         */
        public string ListUnwantedSlots()
        {
            //Listing slots - if a slot in the allocated list is not in the requested list - then this is considered unwanted and will be listed
            List<int> slotsToList = new List<int>();
            List<int> slotsToAvoid = new List<int>(RequestedSlots);
            foreach (int slot in AllocatedSlots)
            {
                if (!slotsToAvoid.Contains(slot))
                {
                    slotsToList.Add(slot);
                }
                else
                {
                    slotsToAvoid.Remove(slot);
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

            List<int> tempRequestedTimeSlots = new List<int>(RequestedSlots);

            double satisfiedslots = 0;
            foreach(int slot in timeSlots)
            {
                if (tempRequestedTimeSlots.Contains(slot))
                {
                    tempRequestedTimeSlots.Remove(slot);
                    satisfiedslots++;
                }
            }

            List<int> gotSlots = RequestedSlots.Where(reqSlot => timeSlots.Any(allocSlot => allocSlot == reqSlot)).ToList();

            return satisfiedslots / numberOfTimeSlotsWanted;
        }
    }
}
