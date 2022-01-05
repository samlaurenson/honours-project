﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActressMas;
using HonoursProject.behaviours;

namespace HonoursProject
{
    class HouseAgent : Agent
    {
        private IBehaviour _agentBehaviour;
        private int _satisfaction;
        private bool _madeInteraction;
        private List<int> _allocatedSlots = new List<int>();
        private List<int> _requestedSlots = new List<int>();

        private static int curve = 0;
        private static int numberOfTimeSlotsWanted = 4;

        public HouseAgent(IBehaviour behaviour)
        {
            this._agentBehaviour = behaviour;
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
