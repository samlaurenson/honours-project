using System;
using System.Collections.Generic;
using System.Text;
using ActressMas;

namespace HonoursProject
{
    class AdvertisingAgent : Agent
    {
        private int _numberOfExchangeRounds;
        private Dictionary<string, List<int>> _advertisedTimeSlots = new Dictionary<string, List<int>>();

        public AdvertisingAgent(int numberOfExchangeRounds)
        {
            this._numberOfExchangeRounds = numberOfExchangeRounds;
        }

        public override void Setup()
        {
            Console.WriteLine("Hello World! - Advertising Agent");
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "list":
                        string nameOfAdvertiser = parameters[0];
                        List<int> advertisedSlots = new List<int>();
                        for (int i = 1; i < parameters.Count; i++)
                        {
                            advertisedSlots.Add(Int32.Parse(parameters[i]));
                        }
                        this._advertisedTimeSlots.Add(nameOfAdvertiser, advertisedSlots);
                        break;
                    case "request":
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
