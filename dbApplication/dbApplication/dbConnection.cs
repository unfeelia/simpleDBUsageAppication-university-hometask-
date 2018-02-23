using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbApplication
{
    public static class DBConnection
    {
        private static string Host = "localhost";
        private static string User = "postgres";
        private static string DBname = "postgres";
        private static string Port = "5432";
        private static string Password = "74gafimo";
        private static string Encod = "UNICODE";
        public static string connString = String.Format("Server={0}; User Id={1}; Database={2}; Port={3}; Password={4}; Client Encoding={5}", Host, User, DBname, Port, Password, Encod);
        /// <summary>return NpgsqlConnection
        /// </summary>
        public static NpgsqlConnection connectToDb()
        {
            return new NpgsqlConnection();
        }
    }
}
