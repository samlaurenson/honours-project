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
            //could have a bool for this function to see whether or not the requesting agent is in the list of agents who this agent owes favours to
            //check for if agent is in list would be done in HouseAgent bit - the decision will be made here
            throw new NotImplementedException();
        }

        //Determines if the agent should switch strategy from selfish to social
        //If agent should switch strategy, then function will return true, otherwise function will return false
        public bool SwitchStrategy()
        {
            throw new NotImplementedException();
        }
    }
}
