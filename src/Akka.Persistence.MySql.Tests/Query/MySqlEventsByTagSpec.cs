//-----------------------------------------------------------------------
// <copyright file="MySqlEventsByTagSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Configuration;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Persistence.TCK.Query;
using Akka.Streams.TestKit;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests.Query
{
    [Collection("MySqlSpec")]
    public class MySqlEventsByTagSpec : EventsByTagSpec
    {
        private static Config Config(MySqlFixture fixture)
        {
            var config = ConfigurationFactory.ParseString($@"
                akka.loglevel = INFO
                akka.persistence {{
                    journal.plugin = ""akka.persistence.journal.mysql""
                    journal.mysql {{
                        event-adapters {{
                            color-tagger  = ""Akka.Persistence.TCK.Query.ColorFruitTagger, Akka.Persistence.TCK""
                        }}
                        event-adapter-bindings = {{
                            ""System.String"" = color-tagger
                        }}
                        class = ""Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql""
                        plugin-dispatcher = ""akka.actor.default-dispatcher""
                        table-name = event_journal
                        auto-initialize = on
                        connection-string = ""{fixture.ConnectionString}""
                        refresh-interval = 1s
                    }}
                    snapshot-store {{
                        plugin = ""akka.persistence.snapshot-store.mysql""
                        mysql {{
                            class = ""Akka.Persistence.MySql.Snapshot.MySqlSnapshotStore, Akka.Persistence.MySql""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                            table-name = snapshot_store
                            auto-initialize = on
                            connection-string = ""{fixture.ConnectionString}""
                        }}
                    }}
                }}
                akka.test.single-expect-default = 10s");
            DbUtils.Initialize(fixture);
            return config;
        }

        public MySqlEventsByTagSpec(ITestOutputHelper output, MySqlFixture fixture) 
            : base(Config(fixture), nameof(MySqlEventsByTagSpec), output)
        {
            ReadJournal = Sys.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
        }

        protected override void AfterAll()
        {
            base.AfterAll();
            DbUtils.Clean();
        }
    }
}
