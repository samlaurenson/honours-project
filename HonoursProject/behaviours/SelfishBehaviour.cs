using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours
{
    public class SelfishBehaviour : IBehaviour
    {
        //Determines if the agent will accept the request exchange or not
        //If agent accepts the exchange, true will be returned, otherwise function will return false
        public bool ConsiderRequest(HouseAgent agent, string requestingAgentName, int requestingAgentSlot, int requestedSlot)
        {
            double currentSatisfaction = agent.CalculateSatisfaction(null);

            List<int> potentialAllocatedTimeSlots = new List<int>(agent.AllocatedSlots);

            //Checking agent still has time slot that requesting agent is looking for
            if (potentialAllocatedTimeSlots.Contains(requestedSlot))
            {
                //Replacing requested slot with the requesting agents unwanted slot
                potentialAllocatedTimeSlots.Remove(requestedSlot);
                potentialAllocatedTimeSlots.Add(requestingAgentSlot);

                double potentialSatisfaction = agent.CalculateSatisfaction(potentialAllocatedTimeSlots);

                //Selfish agents only accept offers that improve their current satisfaction
                if (potentialSatisfaction > currentSatisfaction)
                {
                    return true;
                }
            }
            return false;
        }

        //Determines if the agent should switch strategy from selfish to social
        //If agent should switch strategy, then function will return true, otherwise function will return false
        public bool SwitchStrategy()
        {
            throw new NotImplementedException();
        }
    }
}
