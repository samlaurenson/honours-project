using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ActressMas;

namespace HonoursProject
{
    class AdvertisingAgent : Agent
    {
        private int _numberOfExchangeRounds = 3;
        private Dictionary<string, List<int>> _advertisedTimeSlots = new Dictionary<string, List<int>>();
        private string _currentAdvertisingHouse;
        private int _currentExchangeRound;
        private int lastChances = 0; //Not sure if required - but will be used to re-advertise a households slots if their list has not been emptied.
                                        //households will have 3 last chances before the next household in the list is advertised

        private int _turnsToWait;
        private int _exchangeWaitTimer;
        private bool _exchangeInProgress = false;

        public AdvertisingAgent(int numberOfExchangeRounds)
        {
            this._numberOfExchangeRounds = numberOfExchangeRounds;
        }

        public void DayResetAdvertiser()
        {
            this._currentExchangeRound = 0;
            this._exchangeWaitTimer = 6;
            this._turnsToWait = 6;
            this._exchangeInProgress = false;
        }

        public override void Setup()
        {
            this._exchangeWaitTimer = 3;
            this._turnsToWait = 3;
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "list":
                        this._exchangeWaitTimer = 3;
                        this._turnsToWait = 3;
                        _exchangeInProgress = false;

                        List<int> advertisedSlots = new List<int>();
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            advertisedSlots.Add(Int32.Parse(parameters[i]));
                        }
                        this._advertisedTimeSlots.Add(message.Sender, advertisedSlots);
                        break;
                    case "request":
                        int desiredSlot = Int32.Parse(parameters[0]);
                        int slotToExchange = Int32.Parse(parameters[1]);

                        if (this._advertisedTimeSlots[this._currentAdvertisingHouse].Contains(desiredSlot) && this._currentAdvertisingHouse != message.Sender)
                        {
                            this._advertisedTimeSlots[this._currentAdvertisingHouse].Remove(desiredSlot);

                            //Sending message to agent with the desired slot which contains:
                            // - the name of the house agent that wants the slot
                            // - the slot the requesting agent would want to exchange for the desired slot
                            // - the desired slot the requesting agent wants 
                            Send(this._currentAdvertisingHouse, $"sendRequest {message.Sender} {slotToExchange} {desiredSlot}");
                        }
                        break;
                    case "requestUnsuccessful":
                        //If the exchange did not happen, then a message will be sent back to the advertising agent which contains the slot
                        //to add back to the list of advertised slots of the agent that had the desired slot
                        int slotToReAdd = Int32.Parse(parameters[0]);
                        this._advertisedTimeSlots[message.Sender].Add(slotToReAdd);
                        break;
                    case "newDay":
                        DayResetAdvertiser();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override void ActDefault()
        {
            if (!this._exchangeInProgress && --this._exchangeWaitTimer <= 0)
            {
                //Sending message to agents then waiting for response
                this._exchangeInProgress = true;

                if (this._advertisedTimeSlots.Count > 0)
                {
                    this._currentAdvertisingHouse = this._advertisedTimeSlots.First().Key; //Will increment through list of houses 
                    string currentlyAdvertising = string.Join(" ", this._advertisedTimeSlots[this._currentAdvertisingHouse].ToArray());

                    Broadcast($"notify {currentlyAdvertising}");
                }
            }
            if (this._exchangeInProgress)
            {
                //Waiting for agents to finish their exchange before broadcasting another time slot
                if (--this._turnsToWait <= 0)
                {
                    this._turnsToWait = 6;
                    HandleEnd();
                }
            }
        }

        private void HandleEnd()
        {
            if (string.IsNullOrWhiteSpace(this._currentAdvertisingHouse))
            {
                return;
            }
            Thread.Sleep(50);

            this._advertisedTimeSlots.Remove(this._currentAdvertisingHouse);
            this._currentAdvertisingHouse = "";
            this.lastChances = 0;


            if (this._advertisedTimeSlots.Count > 0)
            {
                this._currentAdvertisingHouse = this._advertisedTimeSlots.First().Key;
                string currentlyAdvertising = string.Join(" ", this._advertisedTimeSlots[this._currentAdvertisingHouse].ToArray());
                //_exchangeInProgress = false;
                Broadcast($"notify {currentlyAdvertising}");
            }
            else
            {
                Console.WriteLine($"----------- End of Round {this._currentExchangeRound} -------------------------");
                this._currentExchangeRound++;
                if (this._currentExchangeRound < _numberOfExchangeRounds)
                {
                    Broadcast("allocate"); //begin next round
                }
                else
                {
                    Broadcast("prepareForNextDay");
                }
            }
        }
    }
}
