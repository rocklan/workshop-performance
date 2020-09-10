using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace perf
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                new Program().WritePeopleOrderedByCountryAsync("output.txt");

                sw.Stop();

                Console.WriteLine("Application took: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("N2") + " seconds");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
            //Console.WriteLine("Press enter to quit...");
            //Console.ReadLine();
        }


        public async Task WritePeopleOrderedByCountryAsync(string outputFileName)
        {
            if (System.IO.File.Exists(outputFileName))
            {
                System.IO.File.Delete(outputFileName);
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 5; i++)
            {
                List<Country> countries = await GetCountriesAsync();

                foreach (Country country in countries)
                {
                    List<Person> people = await GetPeopleFromCountryAsync(country.CountryID);

                    foreach (Person person in people)
                    {
                        string output = country.CountryName + " - " +
                            person.FirstName + " " + person.LastName + Environment.NewLine;

                        sb.Append(output);
                    }
                }
            }

            System.IO.File.AppendAllText(outputFileName, sb.ToString());
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



        public async Task<List<Person>> GetPeopleFromCountryAsync(int? CountryID, int limit = 999999)
        {
            using (System.Data.SqlClient.SqlConnection sc = new System.Data.SqlClient.SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT TOP " + limit +
                             "       p.personid, p.firstname, p.lastname, c.countryname " +
                             "FROM   person p inner join country c on p.countryid = c.countryid" +
                             ((CountryID.HasValue) ? " WHERE p.CountryId=" + CountryID : "");

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    var people = new List<Person>();
                    using (SqlDataReader sqlDataReader = await command.ExecuteReaderAsync())
                    {
                        bool readOk = await sqlDataReader.ReadAsync();
                        while (readOk)
                        {
                            people.Add(new Person(sqlDataReader));
                            readOk = await sqlDataReader.ReadAsync();
                        }
                        sc.Close();
                    }

                    return people;
                }
            }
        }

        public async Task<List<Country>> GetCountriesAsync()
        {
            using (System.Data.SqlClient.SqlConnection sc = new System.Data.SqlClient.SqlConnection(_localDb))
            {
                sc.Open();

                using (SqlCommand command = new SqlCommand("select CountryID, CountryName from country order by CountryName", sc))
                {
                    List<Country> countries = new List<Country>();
                    using (SqlDataReader sqlDataReader = await command.ExecuteReaderAsync())
                    {
                        bool readOk = await sqlDataReader.ReadAsync();
                        while (readOk)
                        {
                            countries.Add(new Country
                            {
                                CountryID = sqlDataReader.GetInt32(0),
                                CountryName = sqlDataReader.GetString(1),
                            });
                            readOk = await sqlDataReader.ReadAsync();
                        }
                        sc.Close();

                        return countries;
                    }
                }
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
