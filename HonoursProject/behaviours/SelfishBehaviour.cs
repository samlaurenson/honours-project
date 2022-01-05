using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours
{
    class SelfishBehaviour : IBehaviour
    {
        //Determines if the agent will accept the request exchange or not
        //If agent accepts the exchange, true will be returned, otherwise function will return false
        public bool ConsiderRequest()
        {
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
