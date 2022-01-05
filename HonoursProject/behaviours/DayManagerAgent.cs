using System;
using System.Collections.Generic;
using System.Text;
using ActressMas;

namespace HonoursProject.behaviours
{
    class DayManagerAgent : Agent
    {
        private int numOfDays = 0; //Increment for how many days have passed in the model
        private List<int> availableTimeSlots = new List<int>();
        public override void Setup()
        {
            CreateAvailableSlots();
            Console.WriteLine("Hello World! - Day manager");
        }

        public override void Act(Message message)
        {
            try
            {
                message.Parse(out string action, out List<string> parameters);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Function that will create available time slots for agents for the day
        private void CreateAvailableSlots()
        {
            int populationSize = Environment.Memory["NoOfAgents"];
            int uniqueTimeSlots = Environment.Memory["UniqueTimeSlots"];

            int requiredTimeSlots = populationSize * uniqueTimeSlots;
            List<int> possibleTimeSlots = new List<int>();

            while (availableTimeSlots.Count < requiredTimeSlots)
            {
                for (int timeSlot = 1; timeSlot <= uniqueTimeSlots; timeSlot++)
                {
                    possibleTimeSlots.Add(timeSlot);
                }

                while (possibleTimeSlots.Count > 0)
                {
                    if (availableTimeSlots.Count < requiredTimeSlots)
                    {
                        Random rand = Environment.Memory["EnvRandom"];
                        int selector = rand.Next(possibleTimeSlots.Count);
                        int timeSlot = possibleTimeSlots[selector];
                        availableTimeSlots.Add(timeSlot);
                        possibleTimeSlots.Remove(selector);
                    }
                    else
                    {
                        possibleTimeSlots.Clear();
                        break;
                    }
                }
            }
            Environment.Memory.Add("AvailableSlots", availableTimeSlots);
        }
    }
}
