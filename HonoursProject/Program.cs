using System;
using ActressMas;

namespace HonoursProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = new EnvironmentMas();
            var houseAg = new HouseAgent();
            env.Add(houseAg, "house");
            env.Start();
        }
    }
}
