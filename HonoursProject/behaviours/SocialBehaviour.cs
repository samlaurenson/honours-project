using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours
{
    //! Class that implements the Social behavioural strategy.
    public class SocialBehaviour : IBehaviour
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
            //bool accept = false;
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
                    //accept = true;
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

                        //If agent owes a favour, then request will be accepted
                        if (favoursOwedToRequester > favoursGivenToRequester)
                        {
                            //Console.WriteLine($"||||||||||||| {agent.Name} REPAID A FAVOUR TO {requestingAgentName} |||||||||||||");

                            return true;
                            //accept = true;
                        }
                    }
                    else
                    {
                        //When social capital is not used, social agents always accept neutral exchanges
                        return true;
                        //accept = true;
                    }
                }

                /*if (DataStore.Instance.HouseAgents.Find(agent => agent.Name == requestingAgentName).Behaviour is SocialBehaviour)
                {
                    /*if (potentialSatisfaction > currentSatisfaction)
                    {
                        return true;
                    }#1#

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

                            //If agent owes a favour, then request will be accepted
                            if (favoursOwedToRequester > favoursGivenToRequester)
                            {
                                //Console.WriteLine($"||||||||||||| {agent.Name} REPAID A FAVOUR TO {requestingAgentName} |||||||||||||");
                                return true;
                            }
                        }
                        else
                        {
                            //When social capital is not used, social agents always accept neutral exchanges
                            return true;
                        }
                    }
                }*/

                /*if (potentialSatisfaction > currentSatisfaction)
                {
                    return true;
                }*/

            }

            return false;
            //return accept;
        }

        //! Function that will switch the behaviour strategy of the agent.
        /*!
         \param agent The house agent that will be switching their behaviour strategy. Social agents will switch to selfish.
         */
        public void SwitchStrategy(HouseAgent agent)
        {
            //Console.WriteLine(agent.Name + " is switching from social to selfish");
            agent.Behaviour = new SelfishBehaviour();
        }
    }
}
