using System;
using System.Diagnostics;

namespace perf
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CountryLogic countryLogic = new CountryLogic();

                Stopwatch sw = Stopwatch.StartNew();

                countryLogic.WritePeopleOrderedByCountry("output.txt");

                sw.Stop();

                Console.WriteLine("Application took: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("N2") + " seconds");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
            Console.WriteLine("Press enter to quit...");
            Console.ReadLine();
        }
    }
}
