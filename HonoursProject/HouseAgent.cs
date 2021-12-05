using System;
using System.Collections.Generic;
using System.Text;
using ActressMas;

namespace HonoursProject
{
    class HouseAgent : Agent
    {
        public override void Setup()
        {
            Console.WriteLine("Hello World!");
            int i = 0;
            while(true)
            {
                Console.WriteLine(i);
                i++;
            }
            Stop();
            //Environment.SimulationFinished();
        }
    }
}
