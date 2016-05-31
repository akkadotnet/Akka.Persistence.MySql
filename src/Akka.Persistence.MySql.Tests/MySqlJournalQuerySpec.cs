//-----------------------------------------------------------------------
// <copyright file="MySqlJournalSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Configuration;
using Akka.Configuration;
using Akka.Persistence.Sql.TestKit;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests
{
    [Collection("MySqlSpec")]
    public class MySqlJournalQuerySpec : SqlJournalQuerySpec
    {
        private static readonly Config SpecConfig;

        static MySqlJournalQuerySpec()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

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
                } " + TimestampConfig("akka.persistence.journal.mysql"));

            DbUtils.Initialize();
        }


        public MySqlJournalQuerySpec(ITestOutputHelper output) : base(SpecConfig, typeof(MySqlJournalQuerySpec).Name, output)
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
