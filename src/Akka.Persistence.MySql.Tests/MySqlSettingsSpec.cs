//-----------------------------------------------------------------------
// <copyright file="MySqlSettingsSpec.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Data;
using Akka.Configuration;
using Akka.Persistence.MySql.Journal;
using Akka.Persistence.MySql.Snapshot;
using Akka.Persistence.Sql.Common;
using Akka.Persistence.Sql.Common.Extensions;
using FluentAssertions;
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
            Assert.Equal("unspecified", config.GetString("read-isolation-level"));
            Assert.Equal("unspecified", config.GetString("write-isolation-level"));
            Assert.Equal("Akka.Persistence.Sql.Common.Journal.DefaultTimestampProvider, Akka.Persistence.Sql.Common", config.GetString("timestamp-provider"));
        }

        [Fact]
        public void MySql_JournalSettings_default_should_contain_default_config()
        {
            var config = MySqlPersistence.Get(Sys).DefaultJournalConfig;
            var settings = new JournalSettings(config);

            // values should be correct
            settings.ConnectionString.Should().Be(string.Empty);
            settings.ConnectionStringName.Should().Be(string.Empty);
            settings.ConnectionTimeout.Should().Be(TimeSpan.FromSeconds(30));
            settings.JournalTableName.Should().Be("event_journal");
            settings.SchemaName.Should().BeNull();
            settings.MetaTableName.Should().Be("metadata");
            settings.TimestampProvider.Should().Be("Akka.Persistence.Sql.Common.Journal.DefaultTimestampProvider, Akka.Persistence.Sql.Common");
            settings.ReadIsolationLevel.Should().Be(IsolationLevel.Unspecified);
            settings.WriteIsolationLevel.Should().Be(IsolationLevel.Unspecified);
            settings.AutoInitialize.Should().BeFalse();

            // values should reflect configuration
            settings.ConnectionString.Should().Be(config.GetString("connection-string"));
            settings.ConnectionStringName.Should().Be(config.GetString("connection-string-name"));
            settings.ConnectionTimeout.Should().Be(config.GetTimeSpan("connection-timeout"));
            settings.JournalTableName.Should().Be(config.GetString("table-name"));
            settings.SchemaName.Should().Be(config.GetString("schema-name", null));
            settings.MetaTableName.Should().Be(config.GetString("metadata-table-name"));
            settings.TimestampProvider.Should().Be(config.GetString("timestamp-provider"));
            settings.ReadIsolationLevel.Should().Be(config.GetIsolationLevel("read-isolation-level"));
            settings.WriteIsolationLevel.Should().Be(config.GetIsolationLevel("write-isolation-level"));
            settings.AutoInitialize.Should().Be(config.GetBoolean("auto-initialize"));
        }
        
        [Fact]
        public void Modified_MySql_JournalSettings_should_contain_proper_config()
        {
            var fullConfig = ConfigurationFactory.ParseString(@"
akka.persistence.journal {
	mysql {
		connection-string = ""a""
		connection-string-name = ""b""
		connection-timeout = 3s
		table-name = ""c""
		auto-initialize = on
		metadata-table-name = ""d""
        schema-name = ""e""
	    serializer = ""f""
		read-isolation-level = snapshot
		write-isolation-level = snapshot
        sequential-access = on
	}
}").WithFallback(MySqlPersistence.DefaultConfiguration());

            var config = fullConfig.GetConfig("akka.persistence.journal.mysql");
            var settings = new JournalSettings(config);
            var executorConfig = MySqlJournal.CreateQueryConfiguration(config, settings);

            // values should be correct
            settings.ConnectionString.Should().Be("a");
            settings.ConnectionStringName.Should().Be("b");
            settings.JournalTableName.Should().Be("c");
            settings.MetaTableName.Should().Be("d");
            settings.SchemaName.Should().Be("e");
            settings.ConnectionTimeout.Should().Be(TimeSpan.FromSeconds(3));
            settings.ReadIsolationLevel.Should().Be(IsolationLevel.Snapshot);
            settings.WriteIsolationLevel.Should().Be(IsolationLevel.Snapshot);
            settings.AutoInitialize.Should().BeTrue();

            executorConfig.JournalEventsTableName.Should().Be("c");
            executorConfig.MetaTableName.Should().Be("d");
            executorConfig.SchemaName.Should().Be("e");
#pragma warning disable CS0618
            executorConfig.DefaultSerializer.Should().Be("f");
#pragma warning restore CS0618
            executorConfig.Timeout.Should().Be(TimeSpan.FromSeconds(3));
            executorConfig.ReadIsolationLevel.Should().Be(IsolationLevel.Snapshot);
            executorConfig.WriteIsolationLevel.Should().Be(IsolationLevel.Snapshot);
            executorConfig.UseSequentialAccess.Should().BeTrue();

            // values should reflect configuration
            settings.ConnectionString.Should().Be(config.GetString("connection-string"));
            settings.ConnectionStringName.Should().Be(config.GetString("connection-string-name"));
            settings.ConnectionTimeout.Should().Be(config.GetTimeSpan("connection-timeout"));
            settings.JournalTableName.Should().Be(config.GetString("table-name"));
            settings.SchemaName.Should().Be(config.GetString("schema-name"));
            settings.MetaTableName.Should().Be(config.GetString("metadata-table-name"));
            settings.ReadIsolationLevel.Should().Be(config.GetIsolationLevel("read-isolation-level"));
            settings.WriteIsolationLevel.Should().Be(config.GetIsolationLevel("write-isolation-level"));
            settings.AutoInitialize.Should().Be(config.GetBoolean("auto-initialize"));

            executorConfig.JournalEventsTableName.Should().Be(config.GetString("table-name"));
            executorConfig.MetaTableName.Should().Be(config.GetString("metadata-table-name"));
            executorConfig.SchemaName.Should().Be(config.GetString("schema-name"));
#pragma warning disable CS0618
            executorConfig.DefaultSerializer.Should().Be(config.GetString("serializer"));
#pragma warning restore CS0618
            executorConfig.Timeout.Should().Be(config.GetTimeSpan("connection-timeout"));
            executorConfig.ReadIsolationLevel.Should().Be(config.GetIsolationLevel("read-isolation-level"));
            executorConfig.WriteIsolationLevel.Should().Be(config.GetIsolationLevel("write-isolation-level"));
            executorConfig.UseSequentialAccess.Should().Be(config.GetBoolean("auto-initialize"));
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
            Assert.Equal("unspecified", config.GetString("read-isolation-level"));
            Assert.Equal("unspecified", config.GetString("write-isolation-level"));
        }
        
        [Fact]
        public void MySql_SnapshotStoreSettings_default_should_contain_default_config()
        {
            var config = MySqlPersistence.Get(Sys).DefaultSnapshotConfig;
            var settings = new SnapshotStoreSettings(config);

            // values should be correct
            settings.ConnectionString.Should().Be(string.Empty);
            settings.ConnectionStringName.Should().Be(string.Empty);
            settings.ConnectionTimeout.Should().Be(TimeSpan.FromSeconds(30));
            settings.SchemaName.Should().BeNull();
            settings.TableName.Should().Be("snapshot_store");
            settings.AutoInitialize.Should().BeFalse();
#pragma warning disable CS0618
            settings.DefaultSerializer.Should().BeNullOrEmpty();
#pragma warning restore CS0618
            settings.ReadIsolationLevel.Should().Be(IsolationLevel.Unspecified);
            settings.WriteIsolationLevel.Should().Be(IsolationLevel.Unspecified);
            settings.FullTableName.Should().Be(settings.TableName);

            // values should reflect configuration
            settings.ConnectionString.Should().Be(config.GetString("connection-string"));
            settings.ConnectionStringName.Should().Be(config.GetString("connection-string-name"));
            settings.ConnectionTimeout.Should().Be(config.GetTimeSpan("connection-timeout"));
            settings.SchemaName.Should().Be(config.GetString("schema-name"));
            settings.TableName.Should().Be(config.GetString("table-name"));
            settings.ReadIsolationLevel.Should().Be(config.GetIsolationLevel("read-isolation-level"));
            settings.WriteIsolationLevel.Should().Be(config.GetIsolationLevel("write-isolation-level"));
            settings.AutoInitialize.Should().Be(config.GetBoolean("auto-initialize"));
#pragma warning disable CS0618
            settings.DefaultSerializer.Should().Be(config.GetString("serializer"));
#pragma warning restore CS0618
        }
        
        [Fact]
        public void Modified_MySql_SnapshotStoreSettings_should_contain_proper_config()
        {
            var fullConfig = ConfigurationFactory.ParseString(@"
akka.persistence.snapshot-store.mysql 
{
	connection-string = ""a""
	connection-string-name = ""b""
	connection-timeout = 3s
	table-name = ""c""
	auto-initialize = on
	serializer = ""d""
    schema-name = ""e""
	sequential-access = on
	read-isolation-level = snapshot
	write-isolation-level = snapshot
}").WithFallback(MySqlPersistence.DefaultConfiguration());
            
            var config = fullConfig.GetConfig("akka.persistence.snapshot-store.mysql");
            var settings = new SnapshotStoreSettings(config);
            var executorConfig = MySqlSnapshotStore.CreateQueryConfiguration(config, settings);

            // values should be correct
            settings.ConnectionString.Should().Be("a");
            settings.ConnectionStringName.Should().Be("b");
            settings.ConnectionTimeout.Should().Be(TimeSpan.FromSeconds(3));
            settings.TableName.Should().Be("c");
#pragma warning disable CS0618
            settings.DefaultSerializer.Should().Be("d");
#pragma warning restore CS0618
            settings.SchemaName.Should().Be("e");
            settings.AutoInitialize.Should().BeTrue();
            settings.ReadIsolationLevel.Should().Be(IsolationLevel.Snapshot);
            settings.WriteIsolationLevel.Should().Be(IsolationLevel.Snapshot);

            executorConfig.SnapshotTableName.Should().Be("c");
#pragma warning disable CS0618
            executorConfig.DefaultSerializer.Should().Be("d");
#pragma warning restore CS0618
            executorConfig.SchemaName.Should().Be("e");
            executorConfig.Timeout.Should().Be(TimeSpan.FromSeconds(3));
            executorConfig.ReadIsolationLevel.Should().Be(IsolationLevel.Snapshot);
            executorConfig.WriteIsolationLevel.Should().Be(IsolationLevel.Snapshot);
            executorConfig.UseSequentialAccess.Should().BeTrue();
            
            // values should reflect configuration
            settings.ConnectionString.Should().Be(config.GetString("connection-string"));
            settings.ConnectionStringName.Should().Be(config.GetString("connection-string-name"));
            settings.ConnectionTimeout.Should().Be(config.GetTimeSpan("connection-timeout"));
            settings.TableName.Should().Be(config.GetString("table-name"));
#pragma warning disable CS0618
            settings.DefaultSerializer.Should().Be(config.GetString("serializer"));
#pragma warning restore CS0618
            settings.SchemaName.Should().Be(config.GetString("schema-name"));
            settings.AutoInitialize.Should().Be(config.GetBoolean("auto-initialize"));
            settings.ReadIsolationLevel.Should().Be(config.GetIsolationLevel("read-isolation-level"));
            settings.WriteIsolationLevel.Should().Be(config.GetIsolationLevel("write-isolation-level"));

            executorConfig.SnapshotTableName.Should().Be(config.GetString("table-name"));
#pragma warning disable CS0618
            executorConfig.DefaultSerializer.Should().Be(config.GetString("serializer"));
#pragma warning restore CS0618
            executorConfig.SchemaName.Should().Be(config.GetString("schema-name"));
            executorConfig.Timeout.Should().Be(config.GetTimeSpan("connection-timeout"));
            executorConfig.ReadIsolationLevel.Should().Be(config.GetIsolationLevel("read-isolation-level"));
            executorConfig.WriteIsolationLevel.Should().Be(config.GetIsolationLevel("write-isolation-level"));
            executorConfig.UseSequentialAccess.Should().Be(config.GetBoolean("sequential-access"));
        }
    }
}
