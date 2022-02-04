using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ActressMas;
using Autofac.Extensions.DependencyInjection;
using HonoursProject.behaviours;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;

namespace HonoursProject
{
    class Program
    {
        public static HttpClient _httpClient = new HttpClient() { };

        async static Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            _httpClient.BaseAddress = new Uri("http://"+configuration["graphing_service"]);

            int uniqueTimeSlots = 24; //Also to go in config file - 1 hour slots
            int maximumSlotCapacity = 16;

            int numberOfSimulationRuns = 4;

            int numberOfHouseholds = 96;
            int numberOfDays = 200;
            //List<int> exchangeRoundList = new List<int>() { 1, 50, 100, 150, 200 };
            //List<int> exchangeRoundList = new List<int>() { 1, 50, 100 };
            List<int> exchangeRoundList = new List<int>() { 100 };

            List<int> listNumberEvolvingAgents = CalculateNumberOfEvolvingAgents(numberOfHouseholds);

            Console.WriteLine("Starting model...");

            //listNumberEvolvingAgents.Count
            for (int i = 0; i < 1; i++)
            {
                int numberOfAgentsEvolving = listNumberEvolvingAgents[i];

                DataStore.Instance.SimulationData.Add(new Dictionary<int, List<List<List<double>>>>());

                //exchangeRoundList.Count
                for (int e = 0; e < exchangeRoundList.Count; e++)
                {
                    for (int j = 0; j < numberOfSimulationRuns; j++)
                    {
                        DataStore.Instance.HouseAgents.Clear();
                        var env = new EnvironmentMas(parallel: true);

                        env.Memory.Add("UniqueTimeSlots", uniqueTimeSlots);
                        env.Memory.Add("NoOfAgents", numberOfHouseholds);
                        env.Memory.Add("MaxSlotCapacity", maximumSlotCapacity);

                        //exchangeRoundList[e]
                        var advertAgent = new AdvertisingAgent(exchangeRoundList[e]);

                        //listNumberEvolvingAgents[i]
                        var dayManager = new DayManagerAgent(48, numberOfDays);

                        for (int k = 0; k < numberOfHouseholds / 2; k++)
                        {
                            var houseAg = new HouseAgent(new SelfishBehaviour(), k);
                            houseAg.SocialCapital = true;
                            DataStore.Instance.HouseAgents.Add(houseAg);
                            env.Add(houseAg, $"house{k}");
                        }

                        for (int k = numberOfHouseholds / 2; k < numberOfHouseholds; k++)
                        {
                            var houseAg = new HouseAgent(new SocialBehaviour(), k);
                            houseAg.SocialCapital = true;
                            DataStore.Instance.HouseAgents.Add(houseAg);
                            env.Add(houseAg, $"house{k}");
                        }

                        env.Add(advertAgent, "advertiser");
                        env.Add(dayManager, "daymanager");

                        env.Start();

                        //Adding end of day data to a dictionary which will store all the satisfactions for a simulation
                        //This includes each of the evolving agent runs which is the "List<Dictionary<int, List<double>>>" - this list contains a dictionary of days, which holds the satisfaction list
                        //for that day

                        //Data storage for model will look like this:
                        //Number of evolving agents
                        //  -> Simulation
                        //      -> Days

                        //So first key will be the simulation, second is a list of the model running with each percentage of evolving agent, then that will contain a dictionary of days with the average satisfactions on that day

                        //Extracting the data for days in this running of the simulation here and will add this to the simulation data dictionary
                        List<List<double>> temp = new List<List<double>>();
                        temp.AddRange(DataStore.Instance.EndOfDaySatisfaction);

                        if (DataStore.Instance.SimulationData[i].ContainsKey(exchangeRoundList[e]))
                        {
                            DataStore.Instance.SimulationData[i][exchangeRoundList[e]].Add(new List<List<double>>(temp));
                        }
                        else
                        {
                            DataStore.Instance.SimulationData[i].Add(exchangeRoundList[e], new List<List<List<double>>>() {temp});
                        }

                        //Temporarily using this so that can simulate with just 100 exchanges
                        /*if (DataStore.Instance.SimulationData[i].ContainsKey(100))
                        {
                            DataStore.Instance.SimulationData[i][100].Add(new List<List<double>>(temp));
                        }
                        else
                        {
                            DataStore.Instance.SimulationData[i].Add(100, new List<List<List<double>>>() { temp });
                        }*/

                        DataStore.Instance.EndOfDaySatisfaction.Clear(); //Clearing list so can be used again

                        //Clearing global favour storage so that favours from previous model runs do not carry over
                        DataStore.Instance.GlobalFavoursOwed.Clear();
                        DataStore.Instance.GlobalFavoursGiven.Clear();

                        Console.WriteLine($"///////// Sim {j + 1} done //////////////");
                    }
                    Console.WriteLine($"Completed model running with {exchangeRoundList[e]} exchange rounds");
                }

                Console.WriteLine($"/////////// {numberOfAgentsEvolving} agents evolving ({i+1}/{listNumberEvolvingAgents.Count}) ////////////");
            }

            //Turning simulation data in to json file which will be sent to the python flask server to produce graphs on the data
            string json = JsonSerializer.Serialize(DataStore.Instance.SimulationData.ToList());

            //File.WriteAllText(@"..\..\..\outputFile.json", json);

            var output = await _httpClient.PostAsync("/graph", new StringContent(json, Encoding.UTF8,"application/json"));

            if (output.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine(await output.Content.ReadAsStringAsync());
            }
        }

        //Function that will determine the number of agents that will evolve in the model
        //Works in the way that the model will run the first time with 0% of agents evolving each day
        //Then another version of the model will run where 10% of agents will evolve each day, and will do this for all the percentages in the percentageOfEvolvingAgents list
        private static List<int> CalculateNumberOfEvolvingAgents(int population)
        {
            List<int> percentageOfEvolvingAgents = new List<int>() { 0, 10, 25, 50, 100 }; //List storing the percentage of agents that will evolve in a model
            List<int> numberOfEvolvingAgents = new List<int>();
            for (int i = 0; i < percentageOfEvolvingAgents.Count; i++)
            {
                numberOfEvolvingAgents.Add((int)Math.Round((population / 100.0f) * percentageOfEvolvingAgents[i]));
            }
            return numberOfEvolvingAgents;
        }
    }
}
