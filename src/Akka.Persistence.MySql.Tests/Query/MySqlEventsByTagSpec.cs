//-----------------------------------------------------------------------
// <copyright file="MySqlEventsByTagSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Configuration;
using Akka.Configuration;
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
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            SpecConfig = ConfigurationFactory.ParseString($@"
                akka.loglevel = INFO
                akka.persistence.journal.plugin = ""akka.persistence.journal.mysql""
                akka.persistence.journal.mysql {{
                    event-adapters {{
                      color-tagger  = ""Akka.Persistence.Sql.TestKit.ColorTagger, Akka.Persistence.Sql.TestKit""
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
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }
    }
}
