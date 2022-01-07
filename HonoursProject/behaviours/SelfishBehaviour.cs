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

        //Function is called when agent will switch from selfish strategy to social strategy
        public void SwitchStrategy(HouseAgent agent)
        {
            Console.WriteLine(agent.Name + " is switching from selfish to social");
            agent.Behaviour = new SocialBehaviour();
        }
    }
}
