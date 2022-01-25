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
        //public static HttpClient _httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:5000/") };
        public static HttpClient _httpClient = new HttpClient() { };


        //{{61.0,64.7,48.6,40.9,36.8,30.2,27.3,23.9,25.2,18.7,15.4,15.6,12.9,10.7,8.5,8.4,9.0,5.7,6.6,6.2,2.3,6.5,3.6,5.5,2.3,3.2,3.5,3.4,3.1,2.2,2.9,2.2,3.4,3.9,5.6,9.1,7.6,6.4,9.8,19.8,30.7,34.5,38.9,45.9,45.5,63.4,73.6,83.7,88.2,96.5,95.6,103.0,112.1,122.7,120.5,117.5,117.8,115.9,118.0,118.8,117.9,126.4,113.8,116.4,106.4,113.9,119.7,114.7,115.4,113.5,116.7,110.9,115.2,118.0,110.3,106.3,100.7,101.3,109.8,112.3,104.3,99.8,98.6,96.0,86.8,87.9,82.9,88.7,88.4,85.7,87.7,88.6,101.7,106.1,105.4,90.3,90.8,89.0,87.6,84.5,82.5,79.8,103.2,100.8,86.4,79.6,82.8,87.7,91.4,98.6,97.3,89.1,84.1,87.8,103.4,120.1,119.2,124.2,119.4,111.7,108.4,105.2,100.4,106.1,110.5,112.0,110.7,107.5,100.3,84.0,86.8,79.6,79.3,75.3,78.9,83.6,89.2,87.8,86.3,80.8,72.9,75.0,70.3,66.0},
        //{8.1,10.2,7.8,7.0,13.3,14.9,12.4,11.0,10.7,8.8,6.7,7.8,10.0,8.2,20.3,36.0,29.9,26.8,21.6,46.0,23.7,19.5,13.7,7.5,6.1,5.6,7.3,8.0,5.3,4.0,3.4,4.5,2.8,6.6,8.0,3.2,8.1,7.2,3.7,5.2,7.9,18.0,16.8,22.4,19.3,18.3,17.2,13.8,21.3,22.9,19.0,32.4,31.6,25.7,23.0,22.6,29.9,30.4,27.3,33.7,27.2,29.0,28.1,31.7,35.5,27.9,21.1,22.3,24.0,21.4,16.7,14.4,21.1,20.3,21.6,22.4,16.7,20.3,12.1,8.6,19.7,24.3,20.3,17.4,13.2,21.3,21.6,16.7,14.4,18.7,20.9,16.6,10.0,7.2,7.3,9.1,10.8,14.3,19.2,18.4,25.8,16.3,14.0,18.2,12.7,17.2,20.6,17.8,24.2,30.3,32.2,24.4,15.6,15.9,17.5,19.5,29.5,24.6,16.3,26.0,20.8,19.2,21.1,27.6,21.5,27.0,24.9,37.4,25.7,29.0,21.1,14.1,21.0,17.6,21.1,15.0,9.7,6.7,9.3,6.8,11.0,11.2,11.2,6.9}};

        async static Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            _httpClient.BaseAddress = new Uri("http://"+configuration["graphing_service"]);

            //Size of array is causing issues when using a double[,], so a list will be used instead -- ideally move this in to a config file
            List<List<double>> DEMAND_CURVE = new List<List<double>>()
            {
                new List<double>(){61.0,64.7,48.6,40.9,36.8,30.2,27.3,23.9,25.2,18.7,15.4,15.6,12.9,10.7,8.5,8.4,9.0,5.7,6.6,6.2,2.3,6.5,3.6,5.5,2.3,3.2,3.5,3.4,3.1,2.2,2.9,2.2,3.4,3.9,5.6,9.1,7.6,6.4,9.8,19.8,30.7,34.5,38.9,45.9,45.5,63.4,73.6,83.7,88.2,96.5,95.6,103.0,112.1,122.7,120.5,117.5,117.8,115.9,118.0,118.8,117.9,126.4,113.8,116.4,106.4,113.9,119.7,114.7,115.4,113.5,116.7,110.9,115.2,118.0,110.3,106.3,100.7,101.3,109.8,112.3,104.3,99.8,98.6,96.0,86.8,87.9,82.9,88.7,88.4,85.7,87.7,88.6,101.7,106.1,105.4,90.3,90.8,89.0,87.6,84.5,82.5,79.8,103.2,100.8,86.4,79.6,82.8,87.7,91.4,98.6,97.3,89.1,84.1,87.8,103.4,120.1,119.2,124.2,119.4,111.7,108.4,105.2,100.4,106.1,110.5,112.0,110.7,107.5,100.3,84.0,86.8,79.6,79.3,75.3,78.9,83.6,89.2,87.8,86.3,80.8,72.9,75.0,70.3,66.0},
                new List<double>(){8.1,10.2,7.8,7.0,13.3,14.9,12.4,11.0,10.7,8.8,6.7,7.8,10.0,8.2,20.3,36.0,29.9,26.8,21.6,46.0,23.7,19.5,13.7,7.5,6.1,5.6,7.3,8.0,5.3,4.0,3.4,4.5,2.8,6.6,8.0,3.2,8.1,7.2,3.7,5.2,7.9,18.0,16.8,22.4,19.3,18.3,17.2,13.8,21.3,22.9,19.0,32.4,31.6,25.7,23.0,22.6,29.9,30.4,27.3,33.7,27.2,29.0,28.1,31.7,35.5,27.9,21.1,22.3,24.0,21.4,16.7,14.4,21.1,20.3,21.6,22.4,16.7,20.3,12.1,8.6,19.7,24.3,20.3,17.4,13.2,21.3,21.6,16.7,14.4,18.7,20.9,16.6,10.0,7.2,7.3,9.1,10.8,14.3,19.2,18.4,25.8,16.3,14.0,18.2,12.7,17.2,20.6,17.8,24.2,30.3,32.2,24.4,15.6,15.9,17.5,19.5,29.5,24.6,16.3,26.0,20.8,19.2,21.1,27.6,21.5,27.0,24.9,37.4,25.7,29.0,21.1,14.1,21.0,17.6,21.1,15.0,9.7,6.7,9.3,6.8,11.0,11.2,11.2,6.9}
            };

            /*List<List<double>> DEMAND_CURVE = new List<List<double>>()
            {
                new List<double>(){61.0,64.7,48.6,40.9,36.8,30.2,27.3,23.9,25.2,18.7,15.4,15.6,12.9,10.7,8.5,8.4,9.0,5.7,6.6,6.2,2.3,6.5,3.6,5.5,2.3,3.2,3.5,3.4,3.1,2.2,2.9,2.2,3.4,3.9,5.6,9.1,7.6,6.4,9.8,19.8,30.7,34.5,38.9,45.9,45.5,63.4,73.6,83.7,88.2,96.5,95.6,103.0,112.1,122.7,120.5,117.5,117.8,115.9,118.0,118.8,117.9,126.4,113.8,116.4,106.4,113.9,119.7,114.7,115.4,113.5,116.7,110.9,115.2,118.0,110.3,106.3,100.7,101.3,109.8,112.3,104.3,99.8,98.6,96.0,86.8,87.9,82.9,88.7,88.4,85.7,87.7,88.6,101.7,106.1,105.4,90.3,90.8,89.0,87.6,84.5,82.5,79.8,103.2,100.8,86.4,79.6,82.8,87.7,91.4,98.6,97.3,89.1,84.1,87.8,103.4,120.1,119.2,124.2,119.4,111.7,108.4,105.2,100.4,106.1,110.5,112.0,110.7,107.5,100.3,84.0,86.8,79.6,79.3,75.3,78.9,83.6,89.2,87.8,86.3,80.8,72.9,75.0,70.3,66.0},
                new List<double>(){8.1,10.2,7.8,7.0,13.3,14.9,12.4,11.0,10.7,8.8,6.7,7.8,10.0,8.2,20.3,36.0,29.9,26.8,21.6,46.0,23.7,19.5,13.7,7.5,6.1,5.6,7.3,8.0,5.3,4.0,3.4,4.5,2.8,6.6,8.0,3.2,8.1,7.2,3.7,5.2,7.9,18.0,16.8,22.4,19.3,18.3,17.2,13.8,21.3,22.9,19.0,32.4,31.6,25.7,23.0,22.6,29.9,30.4,27.3,33.7,27.2,29.0,28.1,31.7,35.5,27.9,21.1,22.3,24.0,21.4,16.7,14.4,21.1,20.3,21.6,22.4,16.7,20.3,12.1,8.6,19.7,24.3,20.3,17.4,13.2,21.3,21.6,16.7,14.4,18.7,20.9,16.6,10.0,7.2,7.3,9.1,10.8,14.3,19.2,18.4,25.8,16.3,14.0,18.2,12.7,17.2,20.6,17.8,24.2,30.3,32.2,24.4,15.6,15.9,17.5,19.5,29.5,24.6,16.3,26.0,20.8,19.2,21.1,27.6,21.5,27.0,24.9,37.4,25.7,29.0,21.1,14.1,21.0,17.6,21.1,15.0,9.7,6.7,9.3,6.8,11.0,11.2,11.2,6.9}
            };*/

            int uniqueTimeSlots = 24; //Also to go in config file - 1 hour slots

            //Creating demand curves and filling them with default values so that they can be accessed through indexes
            List<List<double>> bucketedDemandCurves = Enumerable.Repeat(new List<double>(uniqueTimeSlots), DEMAND_CURVE.Count).ToList();
            List<double> totalDemandValues = Enumerable.Repeat(new double(), DEMAND_CURVE.Count).ToList();

            Random rand = new Random();
            DataStore.Instance.EnvironmentRandom = rand;
            int numberOfSimulationRuns = 10;

            int numberOfHouseholds = 50;
            int numberOfDays = 50;

            List<int> listNumberEvolvingAgents = CalculateNumberOfEvolvingAgents(numberOfHouseholds);

            Console.WriteLine("Starting model...");

            //listNumberEvolvingAgents.Count
            for (int i = 0; i < 1; i++)
            {
                BucketingDemandCurves(ref bucketedDemandCurves, ref totalDemandValues, DEMAND_CURVE, uniqueTimeSlots);
                int numberOfAgentsEvolving = listNumberEvolvingAgents[i];

                for (int j = 0; j < numberOfSimulationRuns; j++)
                {
                    DataStore.Instance.HouseAgents.Clear();
                    var env = new EnvironmentMas();

                    //Demand curves will be used for requesting and receiving time slot allocations
                    //env.Memory.Add("DemandCurve", bucketedDemandCurves);
                    //env.Memory.Add("TotalDemandValues", totalDemandValues);
                    //env.Memory.Add("EnvRandom", rand);
                    DataStore.Instance.BucketedDemandCurve = bucketedDemandCurves;
                    DataStore.Instance.TotalDemand = totalDemandValues;
                    env.Memory.Add("UniqueTimeSlots", uniqueTimeSlots);
                    env.Memory.Add("NoOfAgents", numberOfHouseholds);

                    var advertAgent = new AdvertisingAgent(50);
                    var dayManager = new DayManagerAgent(0, numberOfDays);

                    for (int k = 0; k < numberOfHouseholds/2; k++)
                    {
                        var houseAg = new HouseAgent(new SelfishBehaviour(), k);
                        houseAg.AgentFlexibility = new List<double>() { 1.0 };
                        houseAg.SocialCapital = true;
                        DataStore.Instance.HouseAgents.Add(houseAg);
                        env.Add(houseAg, $"house{k}");
                    }

                    for (int k = numberOfHouseholds / 2; k < numberOfHouseholds; k++)
                    {
                        var houseAg = new HouseAgent(new SocialBehaviour(), k);
                        houseAg.AgentFlexibility = new List<double>() { 1.0 };
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

                    if (DataStore.Instance.SimulationData.ContainsKey(i))
                    {
                        //If an entry has already been made for the simulation, then just add to the list already created
                        DataStore.Instance.SimulationData[i].Add(new List<List<double>>(temp));
                    }
                    else
                    {
                        //If the simulation has not yet had an entry created for it - it will be done here
                        DataStore.Instance.SimulationData.Add(i, new List<List<List<double>>>() { temp });
                    }

                    DataStore.Instance.EndOfDaySatisfaction.Clear(); //Clearing list so can be used again

                    Console.WriteLine($"///////// Sim {j + 1} done //////////////");
                }
                Console.WriteLine($"/////////// {numberOfAgentsEvolving} agents evolving ({i+1}/{listNumberEvolvingAgents.Count}) ////////////");
            }

            //Turning simulation data in to json file which will be sent to the python flask server to produce graphs on the data
            string json = JsonSerializer.Serialize(DataStore.Instance.SimulationData.ToList());
            File.WriteAllText(@"..\..\..\outputFile.json", json);

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

        //Function that will bucket demand curves which will be used to determine what slots are allocated to agents and what slots those agents will request
        //NOTE: Function will be called each time model will be run with new value for evolving agents -- or at least this is how it is done in the inital implementation
        private static void BucketingDemandCurves(ref List<List<double>> bucketedDemandCurves, ref List<double> totalDemandValues, List<List<double>> demandcurve, int uniqueTimeSlots)
        {
            //Re-creating bucketing system like how the initial implementation has it
            for (int i = 0; i < demandcurve.Count; i++)
            {
                List<double> bucketedCurve = Enumerable.Repeat(new double(), uniqueTimeSlots).ToList();
                int bucket = 0;
                int bucketFill = 0;
                for (int j = 0; j < demandcurve[i].Count; j++)
                {
                    bucketedCurve[bucket] = bucketedCurve[bucket] + demandcurve[i][j];
                    bucketFill++;
                    if (bucketFill == 6)
                    {
                        bucketedCurve[bucket] = Math.Round(bucketedCurve[bucket] * 10.0) / 10.0;
                        bucketFill = 0;
                        bucket++;
                    }
                }
                bucketedDemandCurves[i] = bucketedCurve;

                //Calculating total demand
                double totalDemand = 0;
                for (int j = 0; j < bucketedCurve.Count; j++)
                {
                    totalDemand = totalDemand + bucketedCurve[j];
                }

                totalDemand = Math.Round(totalDemand * 10.0) / 10.0;
                totalDemandValues[i] = totalDemand;
            }
        }
    }
}
