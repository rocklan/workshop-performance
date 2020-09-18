using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace perf5
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                new Program().WritePeopleOrderedByCountry("output.txt");

                sw.Stop();

                Console.WriteLine("Application took: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("N2") + " seconds");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
        }


        public void WritePeopleOrderedByCountry(string outputFileName)
        {
            // TODO: Run the profiler then do less database calls, somehow
            // This function is really slow. Surely there must be a faster way 
            // to create a list of all people sorted by country name. Run the
            // profiler and see what's slow. To fix the issue, you might want
            // to see if you can accomplish the same thing with less database calls.

            if (System.IO.File.Exists(outputFileName))
            {
                System.IO.File.Delete(outputFileName);
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                List<Country> countries = GetCountries();

                foreach (Country country in countries)
                {
                    var people = GetPeople(country.CountryID);

                    foreach (var person in people)
                    {
                        sb.Append(person.CountryName);
                        sb.Append(" - ");
                        sb.Append(person.FirstName);
                        sb.Append(" ");
                        sb.Append(person.LastName);
                        sb.Append(Environment.NewLine);
                    }
                }
            }            

            System.IO.File.AppendAllText(outputFileName, sb.ToString());
        }


        public List<Country> GetCountries()
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                using (SqlCommand command = new SqlCommand("select CountryID, CountryName from country order by CountryName", sc))
                {
                    List<Country> countries = new List<Country>();
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            countries.Add(new Country
                            {
                                CountryID = sqlDataReader.GetInt32(0),
                                CountryName = sqlDataReader.GetString(1),
                            });
                        }
                        sc.Close();

                        return countries;
                    }
                }
            }
        }


        public List<Person> GetPeople(int? CountryID = null, int limit = 999999)
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT TOP " + limit +
                             "       p.personid, p.firstname, p.lastname, c.countryname " +
                             "FROM   person p inner join country c on p.countryid = c.countryid" +
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
            this.CountryName = sqlDataReader.GetString(3);
        }

        public override string ToString()
        {
            return this.CountryName + " - " + this.FirstName + " " + this.LastName + Environment.NewLine;
        }

    }

}
