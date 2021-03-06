﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace perf7
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                new Program().CalculateChecksums("encrypted.txt");

                sw.Stop();

                Console.WriteLine("Application took: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("N2") + " seconds");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }


        private void CalculateChecksums(string filename)
        {
            // TODO: Run the profiler then look at the ParallelForEachExample() function
            // This function seems really slow. Run the profiler to see 
            // what's running so slow. To improve the speed, you might 
            // need to take advantage of multiple cores of your CPU. 
            // Check out the function ParallelForEachExample() below 
            // and see what you can do.

            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }

            StringBuilder sb = new StringBuilder();

            int i = 0;
            var people = GetPeople(20000);

            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (Person person in people)
            {
                string encrypted = StringCipher.Encrypt(person.ToString(), "my passphrase is this");

                sb.Append(encrypted);
                sb.Append(Environment.NewLine);

                if ((++i) % 100 == 0)
                    Console.WriteLine(i);
            }

            stopwatch.Stop();

            Console.WriteLine("Execution time of loop: " + (stopwatch.ElapsedMilliseconds / 1000).ToString("N2") + " seconds");
            
        }

        public List<Person> GetPeople(int limit)
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT top " + limit + " p.personid, p.firstname, p.lastname, c.countryname " +
                             "FROM   person p inner join country c on p.countryid = c.countryid " +
                             "order by c.countryName";

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        List<Person> people = new List<Person>();
                        while (sqlDataReader.Read())
                        {
                            people.Add(new Person(sqlDataReader));
                        }
                        return people;
                    }
                }
            }
        }

        private string _localDb
        {
            get
            {
                var path = new FileInfo(System.IO.Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "..", "data"));
                var datafile = path.FullName + "\\peopledb.mdf";

                return "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;AttachDbFileName=" + datafile;
            }
        }


        private void ParallelForEachExample()
        {
            var people = GetPeople(100000);

            ConcurrentBag<int> threads = new ConcurrentBag<int>();

            Parallel.ForEach(people, (person, loopState) =>
            {
                threads.Add(System.Threading.Thread.CurrentThread.ManagedThreadId);

                //TODO: Run the profiler then add code for processing the person here
            });

            Console.WriteLine("Number of unique threads created: " + threads.Distinct().Count());
        }
    }


    public class Person
    {
        public int PersonID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CountryID { get; set; }
        public string CountryName { get; set; }

        public Person(SqlDataReader sqlDataReader)
        {
            this.PersonID = sqlDataReader.GetInt32(0);
            this.FirstName = sqlDataReader.GetString(1);
            this.LastName = sqlDataReader.GetString(2);
            this.CountryName = sqlDataReader.GetString(3);
        }

        public override string ToString()
        {
            return this.CountryName + " - " + this.FirstName + " " + this.LastName + Environment.NewLine;
        }

    }

}
