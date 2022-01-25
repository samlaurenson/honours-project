using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HonoursProject.behaviours
{
    //! Class that implements the Selfish behavioural strategy.
    public class SelfishBehaviour : IBehaviour
    {
        //! Function that will decide whether the agent accepts the exchange request.
        /*!
         \param agent The house agent that is considering the exchange request.
         \param requestingAgentName The name of the house agent that is requesting the exchange.
         \param requestingAgentSlot The slot the requesting agent is wanting to trade.
         \param requestedSlot The slot that the requesting agent want desires.
         \return Decision if the exchange was successful (true) or not (false).
         */
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

        //! Function that will switch the behaviour strategy of the agent.
        /*!
         \param agent The house agent that will be switching their behaviour strategy. Selfish agents will switch to social.
         */
        public void SwitchStrategy(HouseAgent agent)
        {
            //Console.WriteLine(agent.Name + " is switching from selfish to social");
            agent.Behaviour = new SocialBehaviour();
        }
    }
}
