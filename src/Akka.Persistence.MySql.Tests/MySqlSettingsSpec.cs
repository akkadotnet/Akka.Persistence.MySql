//-----------------------------------------------------------------------
// <copyright file="MySqlSettingsSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Xunit;

namespace Akka.Persistence.MySql.Tests
{
    public class MySqlSettingsSpec : Akka.TestKit.Xunit2.TestKit
    {
        [Fact]
        public void MySqlJournalSettings_must_have_default_values()
        {
            MySqlPersistence.Get(Sys);

            var config = Sys.Settings.Config.GetConfig("akka.persistence.journal.mysql");

            Assert.NotNull(config);
            Assert.Equal("akka.persistence.journal.mysql", Sys.Settings.Config.GetString("akka.persistence.journal.plugin"));
            Assert.Equal("Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql", config.GetString("class"));
            Assert.Equal("akka.actor.default-dispatcher", config.GetString("plugin-dispatcher"));
            Assert.Equal(string.Empty, config.GetString("connection-string"));
            Assert.Equal(string.Empty, config.GetString("connection-string-name"));
            Assert.Equal(TimeSpan.FromSeconds(30), config.GetTimeSpan("connection-timeout"));
            Assert.Equal("event_journal", config.GetString("table-name"));
            Assert.Equal("metadata", config.GetString("metadata-table-name"));
            Assert.False(config.GetBoolean("auto-initialize"));
            Assert.Equal("Akka.Persistence.Sql.Common.Journal.DefaultTimestampProvider, Akka.Persistence.Sql.Common", config.GetString("timestamp-provider"));
        }

        [Fact]
        public void MySqlSnapshotStoreSettings_must_have_default_values()
        {
            MySqlPersistence.Get(Sys);

            var config = Sys.Settings.Config.GetConfig("akka.persistence.snapshot-store.mysql");

            Assert.NotNull(config);
            Assert.Equal("akka.persistence.snapshot-store.mysql", Sys.Settings.Config.GetString("akka.persistence.snapshot-store.plugin"));
            Assert.Equal("Akka.Persistence.MySql.Snapshot.MySqlSnapshotStore, Akka.Persistence.MySql", config.GetString("class"));
            Assert.Equal("akka.actor.default-dispatcher", config.GetString("plugin-dispatcher"));
            Assert.Equal(string.Empty, config.GetString("connection-string"));
            Assert.Equal(string.Empty, config.GetString("connection-string-name"));
            Assert.Equal(TimeSpan.FromSeconds(30), config.GetTimeSpan("connection-timeout"));
            Assert.Equal("snapshot_store", config.GetString("table-name"));
            Assert.False(config.GetBoolean("auto-initialize"));
        }
    }
}
