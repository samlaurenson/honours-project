using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject.behaviours
{
    interface IBehaviour
    {
        public bool ConsiderRequest();
        public bool SwitchStrategy();
    }
}
