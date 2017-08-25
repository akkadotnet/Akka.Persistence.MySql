//-----------------------------------------------------------------------
// <copyright file="MySqlSnapshotStoreSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Configuration;
using Akka.Configuration;
using Akka.Persistence.TCK.Snapshot;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests
{
    [Collection("MySqlSpec")]
    public class MySqlSnapshotStoreSpec : SnapshotStoreSpec
    {
        private static readonly Config SpecConfig;

        static MySqlSnapshotStoreSpec()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            SpecConfig = ConfigurationFactory.ParseString(@"
                akka.test.single-expect-default = 3s
                akka.persistence {
                    publish-plugin-commands = on
                    snapshot-store {
                        plugin = ""akka.persistence.snapshot-store.mysql""
                        mysql {
                            class = ""Akka.Persistence.MySql.Snapshot.MySqlSnapshotStore, Akka.Persistence.MySql""
                            plugin-dispatcher = ""akka.actor.default-dispatcher""
                            table-name = snapshot_store
                            auto-initialize = on
                            connection-string = """ + connectionString + @"""
                        }
                    }
                }");

            DbUtils.Initialize();
        }

        public MySqlSnapshotStoreSpec(ITestOutputHelper output)
            : base(SpecConfig, typeof(MySqlSnapshotStoreSpec).Name, output)
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