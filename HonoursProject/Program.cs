using System;
using ActressMas;

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
            TestMe t = new TestMe();
            int test = t.func(2);
            int test2 = t.func2(2);
            var env = new EnvironmentMas();
            var houseAg = new HouseAgent();
            env.Add(houseAg, "house");
            env.Start();
            //Console.ReadLine();
        }
    }
}
