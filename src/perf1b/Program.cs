﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace perf
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                new Program().WriteFirstThousandPeopleMemoryHungry("output.txt");

                sw.Stop();
                
                Console.WriteLine("Application took: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("N2") + " seconds");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
        }




        public void WriteFirstThousandPeopleMemoryHungry(string outputFileName)
        {
            if (System.IO.File.Exists(outputFileName))
            {
                System.IO.File.Delete(outputFileName);
            }

            int peopleFound = 0;
            int PersonID = 1;

            //TODO put a breakpoint here:
            List<Person> people = new List<Person>();
            
            while (peopleFound < 1000)
            {
                Person person = GetPerson(PersonID++);

                if (person != null)
                {
                    people.Add(person);
                    peopleFound++;
                }
            }

            //TODO put another breakpoint here:
            foreach (var person in people)
            {
                System.IO.File.AppendAllText(outputFileName, person.ToString());
            }
        }

        public Person GetPerson(int PersonID)
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT p.personid, p.firstname, p.lastname, c.countryname " +
                             "FROM   person p inner join country c on p.countryid = c.countryid " +
                             "Where p.PersonId = " + PersonID;

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        if (sqlDataReader.Read())
                        {
                            return new Person(sqlDataReader);
                        }
                        return null;
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
