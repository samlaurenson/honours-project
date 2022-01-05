using System;
using System.Collections.Generic;
using System.Text;
using ActressMas;

namespace HonoursProject
{
    class AdvertisingAgent : Agent
    {
        private int _numberOfExchangeRounds;
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
