//-----------------------------------------------------------------------
// <copyright file="MySqlJournalSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Configuration;
using Akka.Persistence.TCK.Journal;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests
{
    [Collection("MySqlSpec")]
    public class MySqlJournalSpec : JournalSpec
    {
        private static Config Config(MySqlFixture fixture)
        {
            var config = ConfigurationFactory.ParseString($@"
                akka.test.single-expect-default = 3s
                akka.persistence {{
                    publish-plugin-commands = on
                    journal {{
                        plugin = ""akka.persistence.journal.mysql""
                        mysql {{
                            class = ""Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                            table-name = event_journal
                            auto-initialize = on
                            connection-string = ""{fixture.ConnectionString}""
                        }}
                    }}
                }}");
            DbUtils.Initialize(fixture);
            return config;
        }

        protected override bool SupportsSerialization => false;

        public MySqlJournalSpec(ITestOutputHelper output, MySqlFixture fixture)
            : base(Config(fixture), nameof(MySqlJournalSpec), output)
        {
            MySqlPersistence.Get(Sys);

            Initialize();
        }

        [Obsolete]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }
    }
}
