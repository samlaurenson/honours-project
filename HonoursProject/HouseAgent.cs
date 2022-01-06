using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActressMas;
using HonoursProject.behaviours;

namespace HonoursProject
{
    public class HouseAgent : Agent
    {
        private IBehaviour _agentBehaviour;
        private int _satisfaction;
        private bool _madeInteraction;
        private bool _useSocialCapital;
        private List<int> _allocatedSlots = new List<int>();
        private List<int> _requestedSlots = new List<int>();
        //private List<string> _favoursOwedTo = new List<string>();
        private Dictionary<string, int> _favoursOwed = new Dictionary<string, int>();
        private Dictionary<string, int> _favoursGiven = new Dictionary<string, int>();

        private static int curve = 0;
        private static int numberOfTimeSlotsWanted = 4;

        private List<double> _agentFlexibility; //Temporarily putting this here until a better place is found
        //Using 2 entries for agent flexibility so that unit tests could be made to fully test the satisfaction calculation function

        public HouseAgent(IBehaviour behaviour)
        {
            this._agentBehaviour = behaviour;
        }

        public IBehaviour Behaviour
        {
            get { return this._agentBehaviour; }
            set { this._agentBehaviour = value; }
        }

        public bool SocialCapital
        {
            get { return this._useSocialCapital; }
            set { this._useSocialCapital = value; }
        }

        public Dictionary<string, int> FavoursOwed
        {
            get { return this._favoursOwed; }
            set { this._favoursOwed = value; }
        }

        public Dictionary<string, int> FavoursGiven
        {
            get { return this._favoursGiven; }
            set { this._favoursGiven = value; }
        }

        /*public List<string> FavoursOwedList
        {
            get { return _favoursOwedTo; }
            set { _favoursOwedTo = value; }
        }*/

        public List<int> AllocatedSlots
        {
            get {return this._allocatedSlots;}
            set { this._allocatedSlots = value; }
        }

        public List<int> RequestedSlots
        {
            get { return this._requestedSlots; }
            set { this._requestedSlots = value; }
        }

        public List<double> AgentFlexibility
        {
            get { return this._agentFlexibility; }
            set { this._agentFlexibility = value; }
        }

        public override void Setup()
        {
            //Make function to do allocation (demand curves from environment memory) and call it here
            List<List<double>> demandCurves = Environment.Memory["DemandCurve"];

            //Agent pick what slots they would like to receive
            RequestingSlotHandler(demandCurves[curve], Environment.Memory["TotalDemandValues"][curve]);

            //Allocating time slot to agent
            RandomSlotAllocationHandler();

            curve++;
            if (curve >= demandCurves.Count)
            {
                curve = 0;
            }

            Console.WriteLine(CalculateSatisfaction(null));
            Console.WriteLine("Hello World!");
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "allocate":
                        //initial allocation of slots for the day
                        break;
                    case "notify":
                        //advertising agent lets house agents know when there is a slot available to exchange
                        break;
                    case "sendRequest":
                        //send a request for a slot exchange
                        break;
                    case "acceptRequest":
                        //receive request for slot exchange
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Function that will calculate the satisfaction an agent would have when given a list of time slots
        //If there is a null input for this function, then the agent will calculate their satisfaction using the list of slots they were allocated
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

        //Function that will select time slots that agent will request based on the demand curve
        private void RequestingSlotHandler(List<double> demandCurve, double totalDemand)
        {
            if (_requestedSlots.Count > 0)
            {
                _requestedSlots.Clear();
            }

            for (int i = 1; i <= numberOfTimeSlotsWanted; i++)
            {
                Random rand = Environment.Memory["EnvRandom"];
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

        //Function that will randomly allocate slots to agent
        private void RandomSlotAllocationHandler()
        {
            List<int> availableTimeSlots = Environment.Memory["AvailableSlots"];
            for (int requestedTimeSlot = 1; requestedTimeSlot <= _requestedSlots.Count; requestedTimeSlot++)
            {
                if (availableTimeSlots.Count > 0)
                {
                    Random rand = Environment.Memory["EnvRandom"];
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
