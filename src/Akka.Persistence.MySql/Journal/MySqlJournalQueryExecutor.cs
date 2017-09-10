//-----------------------------------------------------------------------
// <copyright file="MySqlQueryExecutor.cs" company="Akka.NET Project">
//     Copyright (C) 2016-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Data.Common;
using Akka.Persistence.Sql.Common.Journal;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql.Journal
{
    public class MySqlJournalQueryExecutor : AbstractQueryExecutor
    {
        public MySqlJournalQueryExecutor(QueryConfiguration configuration, Akka.Serialization.Serialization serialization, ITimestampProvider timestampProvider) 
            : base(configuration, serialization, timestampProvider)
        {
            ByTagSql = base.ByTagSql + " LIMIT @Take";

            CreateEventsJournalSql = $@"
                CREATE TABLE IF NOT EXISTS {configuration.FullJournalTableName} (
                    {configuration.OrderingColumnName} BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    {configuration.PersistenceIdColumnName} VARCHAR(255) NOT NULL,
                    {configuration.SequenceNrColumnName} BIGINT NOT NULL,
                    {configuration.IsDeletedColumnName} BIT NOT NULL,
                    {configuration.ManifestColumnName} VARCHAR(500) NOT NULL,
                    {configuration.TimestampColumnName} BIGINT NOT NULL,
                    {configuration.PayloadColumnName} LONGBLOB NOT NULL,
                    {configuration.TagsColumnName} VARCHAR(2000) NULL,
                    {configuration.SerializerIdColumnName} INT,
                    UNIQUE ({configuration.PersistenceIdColumnName}, {configuration.SequenceNrColumnName}),
                    INDEX {configuration.JournalEventsTableName}_sequence_nr_idx ({configuration.SequenceNrColumnName}),
                    INDEX {configuration.JournalEventsTableName}_created_at_idx ({configuration.TimestampColumnName})
                );";

            CreateMetaTableSql = $@"
                CREATE TABLE IF NOT EXISTS {configuration.FullMetaTableName} (
                    {configuration.PersistenceIdColumnName} VARCHAR(255) NOT NULL,
                    {configuration.SequenceNrColumnName} BIGINT NOT NULL,
                    PRIMARY KEY ({configuration.PersistenceIdColumnName}, {configuration.SequenceNrColumnName})
                );";
        }

        protected override DbCommand CreateCommand(DbConnection connection) => new MySqlCommand { Connection = (MySqlConnection)connection };

        protected override string ByTagSql { get; }
        protected override string CreateEventsJournalSql { get; }
        protected override string CreateMetaTableSql { get; }
    }
}
