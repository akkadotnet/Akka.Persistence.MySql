//-----------------------------------------------------------------------
// <copyright file="MySqlSnapshotStore.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Data.Common;
using System.Runtime.CompilerServices;
using Akka.Annotations;
using Akka.Configuration;
using Akka.Persistence.Sql.Common;
using Akka.Persistence.Sql.Common.Snapshot;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql.Snapshot
{
    /// <summary>
    /// Actor used for storing incoming snapshots into persistent snapshot store backed by MySql database.
    /// </summary>
    public class MySqlSnapshotStore : SqlSnapshotStore
    {
        public static readonly MySqlPersistence Extension = MySqlPersistence.Get(Context.System);

        public MySqlSnapshotStore(Config config) : base(config)
        {
            var sqlConfig = config.WithFallback(Extension.DefaultSnapshotConfig);
            QueryExecutor = new MySqlSnapshotQueryExecutor(
                CreateQueryConfiguration(sqlConfig, Settings),
                Context.System.Serialization);
        }

        [InternalApi]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static QueryConfiguration CreateQueryConfiguration(Config config, SnapshotStoreSettings settings)
        {
            return new QueryConfiguration(
                schemaName: config.GetString("schema-name"),
                snapshotTableName: config.GetString("table-name"),
                persistenceIdColumnName: "persistence_id",
                sequenceNrColumnName: "sequence_nr",
                payloadColumnName: "snapshot",
                manifestColumnName: "manifest",
                timestampColumnName: "created_at",
                serializerIdColumnName: "serializer_id",
                timeout: config.GetTimeSpan("connection-timeout"),
                defaultSerializer: config.GetString("serializer"), 
                useSequentialAccess: config.GetBoolean("sequential-access"),
                readIsolationLevel: settings.ReadIsolationLevel,
                writeIsolationLevel: settings.WriteIsolationLevel);
        }

        protected override DbConnection CreateDbConnection(string connectionString) => new MySqlConnection(connectionString);

        public override ISnapshotQueryExecutor QueryExecutor { get; }
    }
}