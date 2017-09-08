//-----------------------------------------------------------------------
// <copyright file="MySqlJournal.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Data.Common;
using Akka.Configuration;
using Akka.Persistence.Sql.Common.Journal;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql.Journal
{
    /// <summary>
    /// Persistent journal actor using MySql as persistence layer. It processes write requests
    /// one by one in synchronous manner, while reading results asynchronously.
    /// </summary>
    public class MySqlJournal : SqlJournal
    {
        public static readonly MySqlPersistence Extension = MySqlPersistence.Get(Context.System);

        public MySqlJournal(Config journalConfig) : base(journalConfig)
        {
            var config = journalConfig.WithFallback(Extension.DefaultJournalConfig);
            QueryExecutor = new MySqlJournalQueryExecutor(new QueryConfiguration(
                schemaName: config.GetString("schema-name"),
                journalEventsTableName: config.GetString("table-name"),
                metaTableName: config.GetString("metadata-table-name"),
                persistenceIdColumnName: "persistence_id",
                sequenceNrColumnName: "sequence_nr",
                payloadColumnName: "payload",
                manifestColumnName: "manifest",
                timestampColumnName: "created_at",
                isDeletedColumnName: "is_deleted",
                tagsColumnName: "tags",
                orderingColumnName: "ordering",
                serializerIdColumnName: "serializer_id",
                timeout: config.GetTimeSpan("connection-timeout"),
                defaultSerializer: config.GetString("default-serializer")),
                    Context.System.Serialization,
                    GetTimestampProvider(config.GetString("timestamp-provider")));
        }

        protected override DbConnection CreateDbConnection(string connectionString) => new MySqlConnection(connectionString);

        protected override string JournalConfigPath => MySqlJournalSettings.ConfigPath;
        public override IJournalQueryExecutor QueryExecutor { get; }
    }
}
