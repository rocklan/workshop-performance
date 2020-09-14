using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace perf6
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                new Program().CalculateChecksums("hashes.txt");

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
            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 20; i++)
            {
                foreach (var person in GetPeople())
                {
                    using (var md5 = MD5.Create())
                    {
                        string name = $"{person.FirstName} {person.LastName}";

                        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(name);
                        byte[] hashBytes = md5.ComputeHash(inputBytes);

                        string hash = Convert.ToBase64String(hashBytes);

                        sb.Append(name);
                        sb.Append(" - ");
                        sb.Append(hash);
                        sb.Append(Environment.NewLine);
                    }
                }
                Console.WriteLine(i);
            }

            System.IO.File.AppendAllText(filename, sb.ToString());
        }


        public List<Person> GetPeople()
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT p.personid, p.firstname, p.lastname, c.countryname " +
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
