using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace perf2
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                new Program().WriteSurnameToFirstNameRatio("output.txt");

                sw.Stop();

                Console.WriteLine("Application took: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("N2") + " seconds");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }


        public void WriteSurnameToFirstNameRatio(string outputFileName)
        {
            // TODO: Run the profiler then fix the bug where exceptions are happening all the time

            // This function isn't quite working properly plus it's really slow. 
            // It should dump out the ratio of
            // the length of the surname to length of firstname.
            // Run the performance profiler, work out what's slow, fix the bug 
            // and check the performance afterwards

            if (System.IO.File.Exists(outputFileName))
            {
                System.IO.File.Delete(outputFileName);
            }

            int i = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var person in GetPeople())
            {
                int firstnameLength = person.FirstName.Trim().Length;
                int surnameLength = person.LastName.Trim().Length;
                decimal ratio;

                sb.Append($"{person.LastName} length to {person.FirstName}  length ratio is ");

                firstnameLength = 0;

                try
                {
                    ratio = surnameLength / firstnameLength;
                    sb.Append(ratio.ToString("N2"));
                }
                catch (Exception)
                {
                    sb.Append("??");
                }

                sb.Append(Environment.NewLine);

                if (++i % 1000 == 0)
                    Console.WriteLine(i);
            }

            System.IO.File.AppendAllText(outputFileName, sb.ToString());
        }


        public List<Person> GetPeople()
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT top 100000 p.personid, p.firstname, p.lastname, c.countryname " +
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
