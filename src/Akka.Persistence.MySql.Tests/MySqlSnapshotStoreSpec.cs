//-----------------------------------------------------------------------
// <copyright file="MySqlSnapshotStoreSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Configuration;
using Akka.Persistence.TCK.Snapshot;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.MySql.Tests
{
    [Collection("MySqlSpec")]
    public class MySqlSnapshotStoreSpec : SnapshotStoreSpec
    {
        private static Config Config(MySqlFixture fixture)
        {
            var config = ConfigurationFactory.ParseString($@"
                akka.test.single-expect-default = 3s
                akka.persistence {{
                    publish-plugin-commands = on
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
                }}");
            DbUtils.Initialize(fixture);
            return config;
        }

        public MySqlSnapshotStoreSpec(ITestOutputHelper output, MySqlFixture fixture)
            : base(Config(fixture), nameof(MySqlSnapshotStoreSpec), output)
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