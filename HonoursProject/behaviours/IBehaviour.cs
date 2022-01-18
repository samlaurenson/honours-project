using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours
{
    //! Interface for behavioural strategies.
    public interface IBehaviour
    {
        //Agent will be used for calculating satisfaction function (which will be public in houseagent class) as well as getting list of favours owed (if required)
        //requestingAgentSlot - one that this agent is being offered by requesting agent
        //requestedSlot - one that this agent will give to the requesting agent

        //! Function that will decide whether the agent accepts the exchange request.
        /*!
         \param agent The house agent that is considering the exchange request.
         \param requestingAgentName The name of the house agent that is requesting the exchange.
         \param requestingAgentSlot The slot the requesting agent is wanting to trade.
         \param requestedSlot The slot that the requesting agent want desires.
         \return Decision if the exchange was successful (true) or not (false).
         */
        public bool ConsiderRequest(HouseAgent agent, string requestingAgentName, int requestingAgentSlot, int requestedSlot);

        //! Function that will switch the behaviour strategy of the agent.
        /*!
         \param agent The house agent that will be switching their behaviour strategy.
         */
        public void SwitchStrategy(HouseAgent agent);
    }
}
