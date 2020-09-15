using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Caching.Memory;

namespace perf3
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                new Program().WritePeopleWithCountryName("output.txt");

                sw.Stop();

                Console.WriteLine("Application took: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("N2") + " seconds");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }

        MemoryCache _myCache = new MemoryCache(new MemoryCacheOptions());


        public void WritePeopleWithCountryName(string outputFileName)
        {
            // This function is really slow. Surely there must be a faster way 
            // to create a list of all people sorted by country name. Run the
            // profiler and see what's slow. To fix the issue, you might want
            // to investigate the GetCountryNameCached() function


            if (System.IO.File.Exists(outputFileName))
            {
                System.IO.File.Delete(outputFileName);
            }

            StringBuilder sb = new StringBuilder();

            int i = 0;
            foreach (var person in GetPeople())
            {
                var countryName = GetCountryName(person.CountryID);

                sb.Append(countryName);
                sb.Append(" - ");
                sb.Append(person.FirstName);
                sb.Append(" ");
                sb.Append(person.LastName);
                sb.Append(Environment.NewLine);

                if (++i % 1000 == 0)
                    Console.WriteLine(i);
            }

            System.IO.File.AppendAllText(outputFileName, sb.ToString());
        }

      
        public string GetCountryName(int CountryID)
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                using (SqlCommand command = new SqlCommand("select CountryName from country where countryId = " + CountryID, sc))
                {
                    List<Country> countries = new List<Country>();
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        string countryName = "";
                        if (sqlDataReader.Read())
                        {
                            countryName = sqlDataReader.GetString(0);
                        }
                        sc.Close();

                        return countryName;
                    }
                }
            }
        }

        public string GetCountryNameCached(int CountryID)
        {
            return _myCache.GetOrCreate<string>(CountryID, (x) =>
            {
                return GetCountryName(CountryID);
            });
        }


        public List<Person> GetPeople(int? CountryID = null, int limit = 999999)
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT TOP " + limit +
                             "       p.personid, p.firstname, p.lastname, p.CountryId " +
                             "FROM   person p " +
                             ((CountryID.HasValue) ? " WHERE p.CountryId=" + CountryID : "");

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    var people = new List<Person>();
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            people.Add(new Person(sqlDataReader));
                        }
                        sc.Close();
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


    }


    public class Country
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
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
            this.CountryID = sqlDataReader.GetInt32(3);
        }

     

    }

}
