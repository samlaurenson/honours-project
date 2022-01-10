using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

        private List<Tuple<string, int, int>> requests = new List<Tuple<string, int, int>>();
 
        private int _turnsToWait;
        private int _exchangeWaitTimer;
        private int _endTimer;
        private bool _pickedReq = false;
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
            this._endTimer = 6;
            this._exchangeInProgress = false;
            this._pickedReq = false;
        }

        public override void Setup()
        {
            this._exchangeWaitTimer = 3;
            this._turnsToWait = 3;
            this._endTimer = 6;
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
                        this._endTimer = 6;
                        _exchangeInProgress = false;
                        _pickedReq = false;

                        List<int> advertisedSlots = new List<int>();
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            advertisedSlots.Add(Int32.Parse(parameters[i]));
                        }
                        this._advertisedTimeSlots.Add(message.Sender, advertisedSlots);
                        break;
                    case "request":
                        //Might have to turn this in to a function which adds to a list of requests and goes through them
                        //When timer actives then could go through the list of requests and deal with them to prevent all agents trying to
                        //request slot from advertiser at same time

                        int desiredSlot = Int32.Parse(parameters[0]);
                        int slotToExchange = Int32.Parse(parameters[1]);

                        requests.Add(new Tuple<string, int, int>(message.Sender, desiredSlot, slotToExchange));
                        break;
                    case "requestUnsuccessful":
                        //If the exchange did not happen, then a message will be sent back to the advertising agent which contains the slot
                        //to add back to the list of advertised slots of the agent that had the desired slot
                        int slotToReAdd = Int32.Parse(parameters[0]);
                        if (this._advertisedTimeSlots.ContainsKey(message.Sender))
                        {
                            this._advertisedTimeSlots[message.Sender].Add(slotToReAdd);
                        }
                        break;
                    case "newDay":
                        DayResetAdvertiser();
                        break;
                    case "Stop":
                        Stop();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void RequestHandler()
        {

            if (requests.Count == 0)
            {
                return;
            }

            //Method to select first requester from the list of requests. This requester will have the chance for their request to be considered by the house that is currently being advertised
            //Other agents who were not selected will be notified and have their status changed back to "not made interaction"
            if (this._advertisedTimeSlots[this._currentAdvertisingHouse].Contains(requests[0].Item2) &&
                this._currentAdvertisingHouse != requests[0].Item1)
            {
                Send(this._currentAdvertisingHouse, $"sendRequest {requests[0].Item1} {requests[0].Item3} {requests[0].Item2}");

                //Letting all unsuccessful agents know that they can request again this round
                /*for (int i = 1; i < requests.Count; i++)
                {
                    Send(requests[i].Item1, "undoInteraction");
                }*/

                //Letting all unsuccessful agents know that they can request again this round -- Accessing the agent variables directly to bypass the computational expense of message passing
                for (int i = 1; i < requests.Count; i++)
                {
                    DataStore.Instance.HouseAgents.Find(agent => agent.Name == requests[i].Item1).MadeInteraction = false;
                }

                requests.Clear();
            }
        }

        public override void ActDefault()
        {
            //Inital timer to start the advertising process for the round
            if (!this._exchangeInProgress && --this._exchangeWaitTimer <= 0)
            {
                //Sending message to agents then waiting for response
                this._exchangeInProgress = true;

                if (this._advertisedTimeSlots.Count > 0)
                {
                    SelectNextAdvertiser();
                    string currentlyAdvertising = string.Join(" ", this._advertisedTimeSlots[this._currentAdvertisingHouse].ToArray());

                    Broadcast($"notify {this._currentAdvertisingHouse} {currentlyAdvertising}");
                }
            }  
            else if (this._exchangeInProgress)
            {
                //Waiting for agents to finish their exchange before broadcasting another time slot
                if (--this._turnsToWait <= 0)
                {
                    this._pickedReq = true;
                    this._turnsToWait = 6;
                    RequestHandler();
                } else if(--this._endTimer<=0 && _pickedReq)
                {
                    //After waiting for agents to accept or decline spots - advertising agent will now pick next house agent to advertise for
                    this._endTimer = 6;
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
            //Thread.Sleep(50);

            this._advertisedTimeSlots.Remove(this._currentAdvertisingHouse);
            this._currentAdvertisingHouse = "";
            this.lastChances = 0;

            SelectNextAdvertiser();

            //Checking to see if there is an agent being advertised. If there is no agent being advertised then round has ended
            if (string.IsNullOrWhiteSpace(this._currentAdvertisingHouse))
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
            else
            {
                string currentlyAdvertising = string.Join(" ", this._advertisedTimeSlots[this._currentAdvertisingHouse].ToArray());
                Broadcast($"notify {this._currentAdvertisingHouse} {currentlyAdvertising}");
            }
        }

        //Function to help select a house agent to advertise slots for
        //If the house agent has already made an interaction then they will not be selected for advertising
        private void SelectNextAdvertiser()
        {
            if (this._advertisedTimeSlots.Count == 0)
            {
                return;
            }

            this._currentAdvertisingHouse = this._advertisedTimeSlots.First().Key;

            //If agent has already made an interaction - find another agent to advertise
            if (DataStore.Instance
                .HouseAgents[
                    DataStore.Instance.HouseAgents.FindIndex(agent => agent.Name == this._currentAdvertisingHouse)]
                .MadeInteraction)
            {
                this._advertisedTimeSlots.Remove(this._currentAdvertisingHouse);
                this._currentAdvertisingHouse = "";
                SelectNextAdvertiser();
            }
        }
    }
}
