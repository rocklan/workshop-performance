using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace perf_hangfire.Controllers
{
    public class PeopleDAO
    { 
        public int GetCount()
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT count(*) as int FROM   person ";

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        List<Person> people = new List<Person>();
                        sqlDataReader.Read();

                        return sqlDataReader.GetInt32(0);
                    }
                }
            }
        }

        public int GetCountHashes()
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT count(*) as int FROM   person where namehash is not null ";

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        List<Person> people = new List<Person>();
                        sqlDataReader.Read();

                        return sqlDataReader.GetInt32(0);
                    }
                }
            }
        }


        public List<Person> GetPeople(int limit, int? startFrom = null)
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "SELECT top " + limit + " p.personid, p.firstname, p.lastname, c.countryname, p.namehash " +
                             "FROM   person p inner join country c on p.countryid = c.countryid ";

                if (startFrom.HasValue)
                    sql += "where p.personid >= " + startFrom.Value + " and p.personid <= " + (startFrom.Value + limit) + " ";

                sql += " order by c.countryName";

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

        internal void UpdateHash(int personID, string hash)
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "update person set namehash=@namehash where personid=@personid";

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    command.Parameters.AddWithValue("@personid", personID);
                    command.Parameters.AddWithValue("@namehash", hash);
                    command.ExecuteNonQuery();
                }
            }
        }

        internal void ClearHashes()
        {
            using (SqlConnection sc = new SqlConnection(_localDb))
            {
                sc.Open();

                string sql = "update person set namehash=null";

                using (SqlCommand command = new SqlCommand(sql, sc))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private string _localDb
        {
            get
            {
                var path = new FileInfo(System.IO.Path.Combine(Environment.CurrentDirectory, "..", "..", "data"));
                var datafile = path.FullName + "\\peopledb.mdf";

                return "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;AttachDbFileName=" + datafile;
            }
        }


    }
}
