using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using ActressMas;

namespace HonoursProject
{
    //! Class for the Advertising agent.
    /*!
     This agent is responsible for notifying house agents of slots that are currently available for exchange.
     Once this agent receives a request for an exchange, the advertiser agent will notify the agent with the desired slot of the request, where the decision will be made to accept or deny the exchange.
     */
    public class AdvertisingAgent : Agent
    {
        private int _numberOfExchangeRounds; /*!< variable that will store the number of exchange rounds that will take place per day */
        private Dictionary<string, List<int>> _advertisedTimeSlots = new Dictionary<string, List<int>>(); /*!< Dictionary that will store the name of the house agent and the list of time slots they wish to advertise */
        private string _currentAdvertisingHouse; /*!< variable that will store the name of the house that has been selected to have their slots advertised */
        private int _currentExchangeRound; /*!< variable that will store the current exchange round */

        private List<Tuple<string, int, int>> requests = new List<Tuple<string, int, int>>(); /*!< variable that will store a list of requests for an agents advertised slots - including the requesting agent name, the desired slot and the slot to exchange for desired slot */

        private int _turnsToWait; /*!< variable that will be the timer which will wait for requesting agents to make a request for an advertised slot */
        private int _exchangeWaitTimer; /*!< variable that will be the timer which will wait for agents to be ready to receive advertisements for the day */
        private int _endTimer; /*!< variable that will be the timer which will wait for agents to make a decision on the exchange request received */
        private bool _pickedReq = false; /*!< variable that will be a flag for signalling a requesting agent process has completed */
        private bool _exchangeInProgress = false; /*!< variable that will be a flag for signalling that the advertiser has started advertising for the day */

        //! Constructor for the advertising agent.
        /*!
         \param numberOfExchangeRounds The maximum number of exchange rounds that will take place in a day.
         */
        public AdvertisingAgent(int numberOfExchangeRounds)
        {
            this._numberOfExchangeRounds = numberOfExchangeRounds;
        }

        //! Getter and setter for advertised slots.
        public Dictionary<string, List<int>> AdvertisedSlots
        {
            get { return _advertisedTimeSlots; }
            set { _advertisedTimeSlots = value; }
        }

        //! Getter and setter for time slot requests
        public List<Tuple<string, int, int>> Requests
        {
            get { return requests; }
            set { requests = value; }
        }

        //! Getter and setter for current advertising house
        public string CurrentAdvertisingHouse
        {
            get { return _currentAdvertisingHouse; }
            set { _currentAdvertisingHouse = value; }
        }

        //! Function to reset the values of the advertising agent.
        /*!
         Function will reset the timers of the advertising agent, the current exchange round counter and flags that determine what point in the exchange the agent is at.
         */
        public void DayResetAdvertiser()
        {
            this._currentExchangeRound = 0;
            this._exchangeWaitTimer = 6;
            this._turnsToWait = 6;
            this._endTimer = 6;
            this._exchangeInProgress = false;
            this._pickedReq = false;
        }

        //! Setup function
        /*!
         Will execute when environment starts.
         Used for setting the timers the advertising agent requires.
         */
        public override void Setup()
        {
            this._exchangeWaitTimer = 3;
            this._turnsToWait = 3;
            this._endTimer = 6;
        }

        //! Act function.
        /*!
         Will run whenever agent receives a message.
         Accepted messages include - "list", "request", "requestUnsuccessful", "newDay", "Stop"
         \param message The message that the agent has received.
         */
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

                        ListHouseTimeSlots(message.Sender, parameters);

                        /*List<int> advertisedSlots = new List<int>();
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            advertisedSlots.Add(Int32.Parse(parameters[i]));
                        }
                        this._advertisedTimeSlots.Add(message.Sender, advertisedSlots);*/
                        break;
                    case "request":
                        //Could change this so that the advertising agent has a public function that will return if there has been a request made or not
                        //and house agents could check this before they request. This could help overcome the problem of adding agents to a list, selecting the first one and 
                        //then having to access and change the unselected agents back to not having made an interaction
                        
                        //could even shuffle the request list before picking who will be considered?

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

        //! Function that will add slots to advertise to a dictionary of households and their unwanted slots
        /*!
         \param sender The household that is listing their unwanted time slots.
         \param parameters The contents of the "list" message sent by the household listing their slots. The message contents contain all the unwanted time slots for the agent.
         */
        public void ListHouseTimeSlots(string sender, List<string> parameters)
        {
            List<int> advertisedSlots = new List<int>();
            for (int i = 0; i < parameters.Count; i++)
            {
                advertisedSlots.Add(Int32.Parse(parameters[i]));
            }
            this._advertisedTimeSlots.Add(sender, advertisedSlots);
        }

        //! Function that will handle incoming requests for time slots
        /*!
         This function will select an agent that will have their exchange request considered.
         The agents that were not selected for consideration will have their made interaction flag reset as they would have not made an interaction.
         \return Message that will be sent to agent who has the desired slot containing the requesting agent name, their proposed slot and what slot they want
         */
        public string RequestHandler()
        {
            string message = "";
            if (requests.Count == 0)
            {
                return message;
            }

            //Method to select first requester from the list of requests. This requester will have the chance for their request to be considered by the house that is currently being advertised
            //Other agents who were not selected will be notified and have their status changed back to "not made interaction"
            if (this._advertisedTimeSlots[this._currentAdvertisingHouse].Contains(requests[0].Item2) &&
                this._currentAdvertisingHouse != requests[0].Item1)
            {
                message = $"sendRequest {requests[0].Item1} {requests[0].Item3} {requests[0].Item2}";

                //Letting all unsuccessful agents know that they can request again this round -- Accessing the agent variables directly to bypass the computational expense of message passing
                for (int i = 1; i < requests.Count; i++)
                {
                    DataStore.Instance.HouseAgents.Find(agent => agent.Name == requests[i].Item1).MadeInteraction = false;
                }

                this._advertisedTimeSlots[this._currentAdvertisingHouse].Remove(requests[0].Item2);

                requests.Clear();

                return message;
            }

            return message;
        }

        //! Act default function.
        /*!
         Function will execute whenever agent has not done anything in a turn.
         This is where advertising agent will check if there are still house agents that are able to have their slots advertised. If so, then that agent will be selected to have their slots advertised.
         */
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
                    Send(this._currentAdvertisingHouse, RequestHandler());
                } else if(--this._endTimer<=0 && _pickedReq)
                {
                    //After waiting for agents to accept or decline spots - advertising agent will now pick next house agent to advertise for
                    this._endTimer = 6;
                    HandleEnd();
                }
            }
        }

        //! Handle end function.
        /*!
         Function will execute when advertising agent has to remove the current advertised house from advertising and select a new house to advertise for.
         Will do this as long as there are still exchange rounds to take place in a day.
         */
        private void HandleEnd()
        {
            if (string.IsNullOrWhiteSpace(this._currentAdvertisingHouse))
            {
                return;
            }
            //Thread.Sleep(50);

            this._advertisedTimeSlots.Remove(this._currentAdvertisingHouse);
            this._currentAdvertisingHouse = "";

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

        //! Function to select the next house to advertise for.
        /*!
         This function will go through the dictionary of time slots to advertise.
         If the agent in the dictionary has already made an interaction this day, then do not advertise the slots for this agent.
         */
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
