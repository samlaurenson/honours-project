using System;
using System.Collections.Generic;
using System.Text;
using ActressMas;

namespace HonoursProject
{
    class AdvertisingAgent : Agent
    {
        private int _numberOfExchangeRounds;
        private Dictionary<string, List<int>> _advertisedTimeSlots = new Dictionary<string, List<int>>();

        public AdvertisingAgent(int numberOfExchangeRounds)
        {
            this._numberOfExchangeRounds = numberOfExchangeRounds;
        }

        public override void Setup()
        {
            Console.WriteLine("Hello World! - Advertising Agent");
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "list":
                        string nameOfAdvertiser = parameters[0];
                        List<int> advertisedSlots = new List<int>();
                        for (int i = 1; i < parameters.Count; i++)
                        {
                            advertisedSlots.Add(Int32.Parse(parameters[i]));
                        }
                        this._advertisedTimeSlots.Add(nameOfAdvertiser, advertisedSlots);
                        break;
                    case "request":
                        int desiredSlot = Int32.Parse(parameters[0]);
                        int slotToExchange = Int32.Parse(parameters[1]);

                        string agentToContact = "";
                            
                        foreach (KeyValuePair<string, List<int>> agent in this._advertisedTimeSlots)
                        {
                            if (agent.Value.Contains(desiredSlot))
                            {
                                agentToContact = agent.Key;
                                agent.Value.Remove(desiredSlot);
                                break;
                            }
                        }

                        //Sending message to agent with the desired slot which contains:
                        // - the name of the house agent that wants the slot
                        // - the slot the requesting agent would want to exchange for the desired slot
                        // - the desired slot the requesting agent wants 
                        Send(agentToContact, $"sendRequest {message.Sender} {slotToExchange} {desiredSlot}");
                        break;
                    case "requestUnsuccessful":
                        //If the exchange did not happen, then a message will be sent back to the advertising agent which contains the slot
                        //to add back to the list of advertised slots of the agent that had the desired slot
                        int slotToReAdd = Int32.Parse(parameters[0]);
                        this._advertisedTimeSlots[message.Sender].Add(slotToReAdd);
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
