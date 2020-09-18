using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace perf9
{

    public class Program
    {
        public static void Main(string[] args)
        {
            // TODO: Execute the program, then add a unit test 

            Summary summary = BenchmarkRunner.Run<StringTest>();
            foreach (var result in summary.Reports)
            {
                Console.WriteLine(result.BenchmarkCase.Descriptor.WorkloadMethod.Name);
                // mean needs to be divided by 1,000,000 then it's in MS
                Console.WriteLine($"Mean: {result.ResultStatistics.Mean}");
            }
        }
    }

    public class StringTest
    {
        [Benchmark]
        public string StringConcat()
        {
            string output = "";
            foreach (var person in GetPeopleEnumerate())
            {
                output += person.ToString();
            }
            return output;
        }

        [Benchmark]
        public string StringBuilder()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var person in GetPeopleEnumerate())
            {
                sb.Append(person.ToString());
            }
            return sb.ToString();
        }

        [Benchmark]
        public string StringBuilderEntirely()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var person in GetPeopleEnumerate())
            {
                sb.Append(person.CountryName);
                sb.Append(" - ");
                sb.Append(person.FirstName);
                sb.Append(" ");
                sb.Append(person.LastName);
                sb.Append(Environment.NewLine);
                sb.Append(person.ToString());
            }
            return sb.ToString();
        }

        private IEnumerable<Person> GetPeopleEnumerate()
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
