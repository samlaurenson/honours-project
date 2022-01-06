using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours
{
    interface IBehaviour
    {
        //Agent will be used for calculating satisfaction function (which will be public in houseagent class) as well as getting list of favours owed (if required)
        public bool ConsiderRequest(HouseAgent agent, string requestingAgentName, int requestingAgentSlot, int requestedSlot);
        public bool SwitchStrategy();
    }
}
