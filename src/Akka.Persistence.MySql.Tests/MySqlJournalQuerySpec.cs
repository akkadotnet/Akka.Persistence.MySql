using System.Configuration;
using Akka.Configuration;
using Akka.Persistence.Sql.Common.TestKit;
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

            var specString = @"
                akka.test.single-expect-default = 15s
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
                } " + TimestampConfig("akka.persistence.journal.mysql");

            SpecConfig = ConfigurationFactory.ParseString(specString);

            //need to make sure db is created before the tests start
            DbUtils.Initialize();
        }

        public MySqlJournalQuerySpec(ITestOutputHelper output)
            : base(SpecConfig, "MySqlJournalQuerySpec", output)
        {
            Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }
    }
}
