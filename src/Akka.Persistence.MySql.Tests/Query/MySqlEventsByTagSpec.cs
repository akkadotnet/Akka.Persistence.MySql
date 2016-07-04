//-----------------------------------------------------------------------
// <copyright file="MySqlEventsByTagSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Configuration;
using Akka.Configuration;
using Akka.Persistence.Query.Sql;
using Akka.Persistence.Sql.TestKit;
using Akka.Util.Internal;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests.Query
{
    public class MySqlEventsByTagSpec : EventsByTagSpec
    {
        public static readonly AtomicCounter Counter = new AtomicCounter(50);

        public static Config CreateSpecConfig()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            return ConfigurationFactory.ParseString($@"
                akka.loglevel = INFO
                akka.persistence.journal.plugin = ""akka.persistence.journal.mysql""
                akka.persistence.journal.mysql {{
                    class = ""Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql""
                    plugin-dispatcher = ""akka.actor.default-dispatcher""
                    table-name = event_journal
                    meta-table-name = journal_metadata
                    auto-initialize = on
                    connection-string = ""{connectionString}""
                    refresh-interval = 1s
                }}
                akka.test.single-expect-default = 10s")
                .WithFallback(SqlReadJournal.DefaultConfiguration());
        }

        public MySqlEventsByTagSpec(ITestOutputHelper output) : base(CreateSpecConfig(), output)
        {
        }
    }
}
