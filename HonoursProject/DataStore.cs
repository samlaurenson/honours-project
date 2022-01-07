using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

namespace HonoursProject
{
    class DataStore
    {
        private static DataStore _instance;
        private List<HouseAgent> _houseAgents = new List<HouseAgent>();
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
    }
}
