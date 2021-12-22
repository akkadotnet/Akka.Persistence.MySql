//-----------------------------------------------------------------------
// <copyright file="MySqlAllPersistenceIdsSpec.cs" company="Akka.NET Project">
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
    public class MySqlAllPersistenceIdsSpec : PersistenceIdsSpec
    {
        private static Config Config(MySqlFixture fixture)
        {
            var config = ConfigurationFactory.ParseString($@"
                akka.loglevel = INFO
                akka.persistence.journal.plugin = ""akka.persistence.journal.mysql""
                akka.persistence.journal.mysql {{
                    class = ""Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql""
                    plugin-dispatcher = ""akka.actor.default-dispatcher""
                    table-name = event_journal
                    auto-initialize = on
                    connection-string = ""{fixture.ConnectionString}""
                    refresh-interval = 1s
                }}
                akka.test.single-expect-default = 10s");
            DbUtils.Initialize(fixture);
            return config;
        }

        public MySqlAllPersistenceIdsSpec(ITestOutputHelper output, MySqlFixture fixture) 
            : base(Config(fixture), nameof(MySqlAllPersistenceIdsSpec), output)
        {
            ReadJournal = Sys.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }
    }
}
