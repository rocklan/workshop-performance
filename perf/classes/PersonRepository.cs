using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace perf
{
    public class PersonRepository
    {
        private string _localDb
        {
            get
            {
                var path = new FileInfo(System.IO.Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "data"));
                var datafile = path.FullName + "\\peopledb.mdf";

                return "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;AttachDbFileName=" + datafile;
            }
        }

        public List<Person> GetPeopleFromCountry(int CountryID, int limit)
        {
            using (System.Data.SqlClient.SqlConnection sc = new System.Data.SqlClient.SqlConnection(_localDb))
            {
                sc.Open();

                using (SqlCommand command = new SqlCommand("select top " + limit + " personid, firstname, lastname from person where CountryId =" + CountryID, sc))
                {
                    var people = new List<Person>();
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            people.Add(new Person
                            {
                                PersonID = sqlDataReader.GetInt32(0),
                                FirstName = sqlDataReader.GetString(1),
                                LastName = sqlDataReader.GetString(2)
                            });
                        }
                        sc.Close();
                    }

                    return people;
                }
            }
        }

        public List<Country> GetCountries()
        {
            using (System.Data.SqlClient.SqlConnection sc = new System.Data.SqlClient.SqlConnection(_localDb))
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
    }
}
