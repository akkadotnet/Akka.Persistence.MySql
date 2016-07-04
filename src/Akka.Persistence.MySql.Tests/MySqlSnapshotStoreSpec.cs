//-----------------------------------------------------------------------
// <copyright file="MySqlSnapshotStoreSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Configuration;
using Akka.Configuration;
using Akka.Persistence.TestKit.Snapshot;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests
{
    [Collection("MySqlSpec")]
    public class MySqlSnapshotStoreSpec : SnapshotStoreSpec
    {
        public MySqlSnapshotStoreSpec(ITestOutputHelper output) : base(CreateSpecConfig())
        {
            MySqlPersistence.Get(Sys);

            Initialize();
        }

        private static Config CreateSpecConfig()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            return ConfigurationFactory.ParseString(@"
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
        }
    }
}