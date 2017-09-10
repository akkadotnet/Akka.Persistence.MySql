//-----------------------------------------------------------------------
// <copyright file="MySqlEventsByTagSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Persistence.TCK.Query;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests.Query
{
    [Collection("MySqlSpec")]
    public class MySqlEventsByTagSpec : EventsByTagSpec
    {
        private static readonly Config SpecConfig;

        static MySqlEventsByTagSpec()
        {
            var connectionString = "Server=127.0.0.1;Port=3306;Database=akka_persistence_tests;User Id=root;Password=Password12!";

            SpecConfig = ConfigurationFactory.ParseString($@"
                akka.loglevel = INFO
                akka.persistence.journal.plugin = ""akka.persistence.journal.mysql""
                akka.persistence.journal.mysql {{
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
                    connection-string = ""{connectionString}""
                    refresh-interval = 1s
                }}
                akka.test.single-expect-default = 10s");
        }

        public MySqlEventsByTagSpec(ITestOutputHelper output) : base(SpecConfig, nameof(MySqlEventsByTagSpec), output)
        {
            DbUtils.Initialize();
            ReadJournal = Sys.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }
    }
}
