using System;
using System.Collections.Generic;
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
        private List<int> allocatedSlots = new List<int>();

        public HouseAgent(IBehaviour behaviour)
        {
            this._agentBehaviour = behaviour;
        }
        public override void Setup()
        {
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
    }
}
