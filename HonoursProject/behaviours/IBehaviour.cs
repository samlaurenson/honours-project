using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours
{
    public interface IBehaviour
    {
        //Agent will be used for calculating satisfaction function (which will be public in houseagent class) as well as getting list of favours owed (if required)
        //requestingAgentSlot - one that this agent is being offered by requesting agent
        //requestedSlot - one that this agent will give to the requesting agent
        public bool ConsiderRequest(HouseAgent agent, string requestingAgentName, int requestingAgentSlot, int requestedSlot);
        public void SwitchStrategy(HouseAgent agent);
    }
}
