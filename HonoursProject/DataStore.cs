using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace HonoursProject
{
    class DataStore
    {
        private static DataStore _instance;
        private List<HouseAgent> _houseAgents = new List<HouseAgent>();
        private Dictionary<int, List<double>> _endOfDaySatisfactions = new Dictionary<int, List<double>>(); //int -> day number, list of doubles -> satisfactions

        private List<int> _availableSlots = new List<int>();
        private Random _random;

        private List<List<double>> bucketedDemandCurves;
        private List<double> totalDemandValues;

        private DataStore() {}

        public static DataStore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataStore();
                }

                return _instance;
            }
        }

        public List<HouseAgent> HouseAgents
        {
            get { return _houseAgents; }
            set { _houseAgents = value; }
        }

        public List<int> AvailableSlots
        {
            get { return _availableSlots; }
            set { _availableSlots = value; }
        }

        public Random EnvironmentRandom
        {
            get { return _random; }
            set { _random = value; }
        }

        public List<double> TotalDemand
        {
            get { return totalDemandValues; }
            set { totalDemandValues = value; }
        }

        public List<List<double>> BucketedDemandCurve
        {
            get { return bucketedDemandCurves; }
            set { bucketedDemandCurves = value; }
        }

        public Dictionary<int, List<double>> EndOfDaySatisfactoin
        {
            get { return _endOfDaySatisfactions; }
            set { _endOfDaySatisfactions = value; }
        }

        public void CalculateEndOfDaySatisfactions(int day)
        {
            List<double> satisfactions = new List<double>();

            satisfactions.Add(averageAgentSatisfaction());
            satisfactions.Add(optimumAgentSatisfaction());

            //Adding average satisfaction of each agent type to list
            calculateSatisfactionForAgentTypes(satisfactions);

            //Getting the average variance for each agent type
            endOfDaySatisfactionStandardDeviation(satisfactions);

            _endOfDaySatisfactions.Add(day, satisfactions);
        }

        private double averageAgentSatisfaction()
        {
            return 0;
        }

        private double optimumAgentSatisfaction()
        {
            return 0;
        }

        private void calculateSatisfactionForAgentTypes(List<double> satisfactions)
        {

        }

        private void endOfDaySatisfactionStandardDeviation(List<double> satisfactions)
        {

        }
    }
}
