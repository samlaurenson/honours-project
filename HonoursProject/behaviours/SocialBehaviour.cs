using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours
{
    public class SocialBehaviour : IBehaviour
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

                if (potentialSatisfaction > currentSatisfaction)
                {
                    return true;
                }

                if (Equals(potentialSatisfaction, currentSatisfaction))
                {
                    if (agent.SocialCapital)
                    {
                        int favoursOwedToRequester = 0;
                        int favoursGivenToRequester = 0;

                        if (agent.FavoursOwed.ContainsKey(requestingAgentName))
                        {
                            favoursOwedToRequester = agent.FavoursOwed[requestingAgentName];
                        }

                        if (agent.FavoursGiven.ContainsKey(requestingAgentName))
                        {
                            favoursGivenToRequester = agent.FavoursGiven[requestingAgentName];
                        }

                        if (favoursOwedToRequester > favoursGivenToRequester)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        //When social capital is not used, social agents always accept neutral exchanges
                        return true;
                    }
                }
            }
            return false;
        }

        //Function is called when agent will switch from social strategy to selfish strategy
        public void SwitchStrategy(HouseAgent agent)
        {
            agent.Behaviour = new SelfishBehaviour();
        }
    }
}
