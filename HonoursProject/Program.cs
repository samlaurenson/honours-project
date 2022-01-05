using System;
using ActressMas;
using HonoursProject.behaviours;

namespace HonoursProject
{
    public class TestMe
    {
        public int func(int t)
        {
            return t + 1;
        }

        public int func2(int t)
        {
            return t - 1;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            /*TestMe t = new TestMe();
            int test = t.func(2);
            int test2 = t.func2(2);*/
            var env = new EnvironmentMas();
            var advertAgent = new AdvertisingAgent(3);
            var houseAg = new HouseAgent(new SelfishBehaviour());

            env.Add(advertAgent, "advertiser");
            env.Add(houseAg, "house");

            env.Start();
            //Console.ReadLine();
        }
    }
}
