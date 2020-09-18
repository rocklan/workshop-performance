using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace perf9b
{
    public class Tests
    {
        [Test]
        public void ConcatenatePeoplesNames_Should_Run_Under_200ms()
        {
            Summary summary = BenchmarkRunner.Run<StringTest>();
            double resultInMs = summary.Reports[0].ResultStatistics.Mean / 1000000;
            Assert.Less(resultInMs, 200);
        }

        // TODO: Add unit test to validate ComputeHashForNames is performing ok
    }


    public class StringTest
    {
        [Benchmark]
        public string ConcatenatePeoplesNames()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var person in new PeopleDAO().GetPeopleEnumerate())
            {
                sb.Append(person.ToString());
            }
            return sb.ToString();
        }
    }

    public class Md5Test
    {
        [Benchmark]
        public void ComputeHashForNames()
        {
            MD5 md5 = MD5.Create();
            foreach (var person in new PeopleDAO().GetPeopleEnumerate())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(person.ToString());
                
                md5.ComputeHash(inputBytes);
            }
        }
    }

    public class PeopleDAO
    { 
        public IEnumerable<Person> GetPeopleEnumerate()
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT top 10000 p.personid, p.firstname, p.lastname, c.countryname " +
                             "FROM   person p inner join country c on p.countryid = c.countryid " +
                             "order by c.countryName";

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            yield return new Person(sqlDataReader);
                        }
                    }
                }
            }
        }

        private string _localDb
        {
            get
            {
                var path = new FileInfo(System.IO.Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "..", "..", "..", "..", "..", "data"));
                var datafile = path.FullName + "\\peopledb.mdf";

                return "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;AttachDbFileName=" + datafile;
            }
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