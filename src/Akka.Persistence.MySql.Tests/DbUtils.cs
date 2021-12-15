//-----------------------------------------------------------------------
// <copyright file="DbUtils.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql.Tests
{
    public static class DbUtils
    {
        private static MySqlFixture Fixture;
        
        public static void Initialize(MySqlFixture fixture)
        {
            Fixture = fixture;
            //var connectionString = "Server=127.0.0.1;Port=3306;Database=akka_persistence_tests;User Id=root;Password=Password12!";
            // var connectionBuilder = new MySqlConnectionStringBuilder(fixture.ConnectionString);

            //connect to mysql database to create a new database
            // var databaseName = connectionBuilder.Database;
            // connectionBuilder.Database = databaseName;
            // connectionString = connectionBuilder.ToString();

            using (var conn = new MySqlConnection(fixture.ConnectionString))
            {
                conn.Open();

                bool dbExists;
                using (var cmd = new MySqlCommand())
                {
                    cmd.CommandText =
                        $@"SELECT true FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{MySqlFixture.DatabaseName}'";
                    cmd.Connection = conn;

                    var result = cmd.ExecuteScalar();
                    dbExists = result != null && Convert.ToBoolean(result);
                }

                if (dbExists)
                {
                    DropTables(conn);
                }
                else
                {
                    DoCreate(conn, MySqlFixture.DatabaseName);
                }
            }
        }

        public static void Clean()
        {
            //var connectionString = "Server=127.0.0.1;Port=3306;Database=akka_persistence_tests;User Id=root;Password=Password12!";

            using (var conn = new MySqlConnection(Fixture.ConnectionString))
            {
                conn.Open();
                DropTables(conn);
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

        private static void DropTables(MySqlConnection conn)
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
