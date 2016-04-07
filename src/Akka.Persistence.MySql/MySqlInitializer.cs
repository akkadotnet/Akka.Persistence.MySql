using System;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql
{
    internal static class MySqlInitializer
    {
        private const string SqlJournalFormat = @"
            CREATE TABLE IF NOT EXISTS {0} (
                persistence_id VARCHAR(255) NOT NULL,
                sequence_nr BIGINT NOT NULL,
                is_deleted BIT NOT NULL,
                created_at BIGINT NOT NULL,
                manifest VARCHAR(500) NOT NULL,
                payload BLOB NOT NULL,
                PRIMARY KEY (persistence_id, sequence_nr),
                INDEX {1}_sequence_nr_idx (sequence_nr),
                INDEX {1}_created_at_idx (created_at)
            );";

        private const string SqlSnapshotStoreFormat = @"
            CREATE TABLE IF NOT EXISTS {0} (
                persistence_id VARCHAR(255) NOT NULL,
                sequence_nr BIGINT NOT NULL,
                created_at BIGINT NOT NULL,
                manifest VARCHAR(500) NOT NULL,
                snapshot BLOB NOT NULL,
                PRIMARY KEY (persistence_id, sequence_nr),
                INDEX {1}_sequence_nr_idx (sequence_nr),
                INDEX {1}_created_at_idx (created_at)
            );";

        private const string SqlMetadataFormat = @"
            CREATE TABLE IF NOT EXISTS {0} (
                persistence_id VARCHAR(255) NOT NULL,
                sequence_nr BIGINT NOT NULL,
                PRIMARY KEY (persistence_id, sequence_nr)
            );";

        /// <summary>
        /// Initializes a MySql journal-related tables according to 'table-name' 
        /// and 'connection-string' values provided in 'akka.persistence.journal.mysql' config.
        /// </summary>
        internal static void CreateMySqlJournalTables(string connectionString, string tableName)
        {
            var sql = InitJournalSql(tableName);
            ExecuteSql(connectionString, sql);
        }

        /// <summary>
        /// Initializes a MySql snapshot store related tables according to 'table-name' 
        /// and 'connection-string' values provided in 'akka.persistence.snapshot-store.mysql' config.
        /// </summary>
        internal static void CreateMySqlSnapshotStoreTables(string connectionString, string tableName)
        {
            var sql = InitSnapshotStoreSql(tableName);
            ExecuteSql(connectionString, sql);
        }

        /// <summary>
        /// Initializes a MySql metadata table according to 'schema-name', 'metadata-table-name' 
        /// and 'connection-string' values provided in 'akka.persistence.journal.mysql' config.
        /// </summary>
        internal static void CreateMySqlMetadataTables(string connectionString, string metadataTableName)
        {
            var sql = InitMetadataSql(metadataTableName);
            ExecuteSql(connectionString, sql);
        }

        private static string InitJournalSql(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "Akka.Persistence.MySql journal table name is required");

            var cb = new MySqlCommandBuilder();
            return string.Format(SqlJournalFormat, cb.QuoteIdentifier(tableName), cb.UnquoteIdentifier(tableName));
        }

        private static string InitSnapshotStoreSql(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "Akka.Persistence.MySql snapshot store table name is required");

            var cb = new MySqlCommandBuilder();
            return string.Format(SqlSnapshotStoreFormat, cb.QuoteIdentifier(tableName), cb.UnquoteIdentifier(tableName));
        }

        private static string InitMetadataSql(string metadataTable)
        {
            if (string.IsNullOrEmpty(metadataTable)) throw new ArgumentNullException("metadataTable", "Akka.Persistence.MySql metadata table name is required");

            var cb = new MySqlCommandBuilder();
            return string.Format(SqlMetadataFormat, cb.QuoteIdentifier(metadataTable));
        }

        private static void ExecuteSql(string connectionString, string sql)
        {
            using (var conn = new MySqlConnection(connectionString))
            using (var command = conn.CreateCommand())
            {
                conn.Open();

                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }
    }
}