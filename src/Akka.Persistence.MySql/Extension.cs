using System;
using System.Configuration;
using Akka.Actor;
using Akka.Configuration;
using Akka.Persistence.Sql.Common;

namespace Akka.Persistence.MySql
{
    /// <summary>
    /// Configuration settings representation targeting MySql journal actor.
    /// </summary>
    public class MySqlJournalSettings : JournalSettings
    {
        public const string JournalConfigPath = "akka.persistence.journal.mysql";

        /// <summary>
        /// Flag determining in case of event journal table missing, it should be automatically initialized.
        /// </summary>
        public bool AutoInitialize { get; private set; }

        /// <summary>
        /// Metadata table name
        /// </summary>
        public string MetadataTableName { get; private set; }

        public MySqlJournalSettings(Config config)
            : base(config)
        {
            AutoInitialize = config.GetBoolean("auto-initialize");
            MetadataTableName = config.GetString("metadata-table-name");
        }
    }

    /// <summary>
    /// Configuration settings representation targeting MySql snapshot store actor.
    /// </summary>
    public class MySqlSnapshotStoreSettings : SnapshotStoreSettings
    {
        public const string SnapshotStoreConfigPath = "akka.persistence.snapshot-store.mysql";

        /// <summary>
        /// Flag determining in case of snapshot store table missing, it should be automatically initialized.
        /// </summary>
        public bool AutoInitialize { get; private set; }

        public MySqlSnapshotStoreSettings(Config config)
            : base(config)
        {
            AutoInitialize = config.GetBoolean("auto-initialize");
        }
    }

    /// <summary>
    /// An actor system extension initializing support for MySql persistence layer.
    /// </summary>
    public class MySqlPersistence : IExtension
    {
        /// <summary>
        /// Returns a default configuration for akka persistence SQLite-based journals and snapshot stores.
        /// </summary>
        /// <returns></returns>
        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<MySqlPersistence>("Akka.Persistence.MySql.mysql.conf");
        }

        public static MySqlPersistence Get(ActorSystem system)
        {
            return system.WithExtension<MySqlPersistence, MySqlPersistenceProvider>();
        }

        /// <summary>
        /// Journal-related settings loaded from HOCON configuration.
        /// </summary>
        public readonly MySqlJournalSettings JournalSettings;

        /// <summary>
        /// Snapshot store related settings loaded from HOCON configuration.
        /// </summary>
        public readonly MySqlSnapshotStoreSettings SnapshotSettings;

        public MySqlPersistence(ExtendedActorSystem system)
        {
            system.Settings.InjectTopLevelFallback(DefaultConfiguration());

            JournalSettings = new MySqlJournalSettings(system.Settings.Config.GetConfig(MySqlJournalSettings.JournalConfigPath));
            SnapshotSettings = new MySqlSnapshotStoreSettings(system.Settings.Config.GetConfig(MySqlSnapshotStoreSettings.SnapshotStoreConfigPath));

            if (JournalSettings.AutoInitialize)
            {
                var connectionString = string.IsNullOrEmpty(JournalSettings.ConnectionString)
                    ? ConfigurationManager.ConnectionStrings[JournalSettings.ConnectionStringName].ConnectionString
                    : JournalSettings.ConnectionString;

                MySqlInitializer.CreateMySqlJournalTables(connectionString, JournalSettings.TableName);
                MySqlInitializer.CreateMySqlMetadataTables(connectionString, JournalSettings.MetadataTableName);
            }

            if (SnapshotSettings.AutoInitialize)
            {
                var connectionString = string.IsNullOrEmpty(SnapshotSettings.ConnectionString)
                    ? ConfigurationManager.ConnectionStrings[SnapshotSettings.ConnectionStringName].ConnectionString
                    : SnapshotSettings.ConnectionString;

                MySqlInitializer.CreateMySqlSnapshotStoreTables(connectionString, SnapshotSettings.TableName);
            }
        }
    }

    /// <summary>
    /// Singleton class used to setup MySql backend for akka persistence plugin.
    /// </summary>
    public class MySqlPersistenceProvider : ExtensionIdProvider<MySqlPersistence>
    {
        
        /// <summary>
        /// Creates an actor system extension for akka persistence MySql support.
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public override MySqlPersistence CreateExtension(ExtendedActorSystem system)
        {
            return new MySqlPersistence(system);
        }
    }
}