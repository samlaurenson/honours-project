using System;
using System.Collections.Generic;
using System.Text;

namespace HonoursProject
{
    class DataStore
    {
        private static DataStore _instance;

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
    }
}
