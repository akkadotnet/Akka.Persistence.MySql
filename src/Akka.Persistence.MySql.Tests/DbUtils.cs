using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql.Tests
{
    public static class DbUtils
    {
        public static void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            var connectionBuilder = new MySqlConnectionStringBuilder(connectionString);

            //connect to mysql database to create a new database
            var databaseName = connectionBuilder.Database;
            connectionBuilder.Database = databaseName;
            connectionString = connectionBuilder.ToString();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                bool dbExists;
                using (var cmd = new MySqlCommand())
                {
                    cmd.CommandText = string.Format(@"SELECT true FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", databaseName);
                    cmd.Connection = conn;

                    var result = cmd.ExecuteScalar();
                    dbExists = result != null && Convert.ToBoolean(result);
                }

                if (dbExists)
                {
                    DoClean(conn);
                }
                else
                {
                    DoCreate(conn, databaseName);
                }
            }
        }

        public static void Clean()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                DoClean(conn);
            }
        }

        private static void DoCreate(MySqlConnection conn, string databaseName)
        {
            using (var cmd = new MySqlCommand())
            {
                cmd.CommandText = string.Format(@"CREATE DATABASE {0}", databaseName);
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
        }

        private static void DoClean(MySqlConnection conn)
        {
            using (var cmd = new MySqlCommand())
            {
                cmd.CommandText = @"
                    DROP TABLE IF EXISTS event_journal;
                    DROP TABLE IF EXISTS snapshot_store;
                    DROP TABLE IF EXISTS metadata";
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
        }
    }
}