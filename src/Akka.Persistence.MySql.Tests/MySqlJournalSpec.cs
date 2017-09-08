//-----------------------------------------------------------------------
// <copyright file="MySqlJournalSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration;
using Akka.Persistence.TCK.Journal;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests
{
    [Collection("MySqlSpec")]
    public class MySqlJournalSpec : JournalSpec
    {
        private static readonly Config SpecConfig;

        static MySqlJournalSpec()
        {
            var connectionString = "Server=127.0.0.1;Port=3306;Database=akka_persistence_tests;User Id=root;Password=Password12!";

            SpecConfig = ConfigurationFactory.ParseString(@"
                akka.test.single-expect-default = 3s
                akka.persistence {
                    publish-plugin-commands = on
                    journal {
                        plugin = ""akka.persistence.journal.mysql""
                        mysql {
                            class = ""Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                            table-name = event_journal
                            auto-initialize = on
                            connection-string = """ + connectionString + @"""
                        }
                    }
                }");

            DbUtils.Initialize();
        }

        public MySqlJournalSpec(ITestOutputHelper output)
            : base(SpecConfig, typeof(MySqlJournalSpec).Name, output)
        {
            MySqlPersistence.Get(Sys);

            Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }
    }
}
