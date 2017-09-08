//-----------------------------------------------------------------------
// <copyright file="Extension.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

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
        public const string ConfigPath = "akka.persistence.journal.mysql";

        public MySqlJournalSettings(Config config) : base(config)
        {
        }
    }

    /// <summary>
    /// Configuration settings representation targeting MySql snapshot store actor.
    /// </summary>
    public class MySqlSnapshotStoreSettings : SnapshotStoreSettings
    {
        public const string ConfigPath = "akka.persistence.snapshot-store.mysql";

        public MySqlSnapshotStoreSettings(Config config) : base(config)
        {
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
            return ConfigurationFactory.FromResource<MySqlPersistence>("Akka.Persistence.MySql.reference.conf");
        }

        public static MySqlPersistence Get(ActorSystem system)
        {
            return system.WithExtension<MySqlPersistence, MySqlPersistenceProvider>();
        }

        /// <summary>
        /// Journal-related settings loaded from HOCON configuration.
        /// </summary>
        public readonly Config DefaultJournalConfig;

        /// <summary>
        /// Snapshot store related settings loaded from HOCON configuration.
        /// </summary>
        public readonly Config DefaultSnapshotConfig;

        public MySqlPersistence(ExtendedActorSystem system)
        {
            var defaultConfig = DefaultConfiguration();
            system.Settings.InjectTopLevelFallback(defaultConfig);

            DefaultJournalConfig = defaultConfig.GetConfig(MySqlJournalSettings.ConfigPath);
            DefaultSnapshotConfig = defaultConfig.GetConfig(MySqlSnapshotStoreSettings.ConfigPath);
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