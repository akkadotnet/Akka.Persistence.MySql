using System;
using System.Data.Common;
using System.Text;
using Akka.Persistence.Sql.Common.Snapshot;
using MySql.Data.MySqlClient;


namespace Akka.Persistence.MySql.Snapshot
{
    internal class MySqlSnapshotQueryBuilder : ISnapshotQueryBuilder
    {
        private readonly string _deleteSql;
        private readonly string _selectSql;
        private readonly string _insertSql;

        public MySqlSnapshotQueryBuilder(MySqlSnapshotStoreSettings settings)
        {
            var tableName = settings.TableName;

            _deleteSql = @"DELETE FROM {0} WHERE persistence_id = @persistence_id ".QuoteTable(tableName);
            _selectSql = @"SELECT persistence_id, sequence_nr, created_at as created_at, manifest, snapshot FROM {0} WHERE persistence_id = @persistence_id ".QuoteTable(tableName);
            _insertSql = @"INSERT INTO {0} (persistence_id, sequence_nr, created_at, manifest, snapshot) VALUES (@persistence_id, @sequence_nr, @created_at, @manifest, @snapshot) ON DUPLICATE KEY UPDATE created_at = @created_at, snapshot = @snapshot".QuoteTable(tableName);
        }

        public DbCommand DeleteOne(string persistenceId, long sequenceNr, DateTime timestamp)
        {
            var sqlCommand = new MySqlCommand();
            sqlCommand.Parameters.Add(new MySqlParameter("@persistence_id", MySqlDbType.VarChar, persistenceId.Length)
            {
                Value = persistenceId
            });
            var sb = new StringBuilder(_deleteSql);

            if (sequenceNr < long.MaxValue && sequenceNr > 0)
            {
                sb.Append(@"AND sequence_nr = @sequence_nr ");
                sqlCommand.Parameters.Add(new MySqlParameter("@sequence_nr", MySqlDbType.Int64) {Value = sequenceNr});
            }

            if (timestamp > DateTime.MinValue && timestamp < DateTime.MaxValue)
            {
                sb.Append(@"AND created_at = @created_at");
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at", MySqlDbType.Int64)
                {
                    Value = timestamp.Ticks
                });
            }

            sqlCommand.CommandText = sb.ToString();

            return sqlCommand;
        }

        public DbCommand DeleteMany(string persistenceId, long maxSequenceNr, DateTime maxTimestamp)
        {
            var sqlCommand = new MySqlCommand();
            sqlCommand.Parameters.Add(new MySqlParameter("@persistence_id", MySqlDbType.VarChar, persistenceId.Length)
            {
                Value = persistenceId
            });
            var sb = new StringBuilder(_deleteSql);

            if (maxSequenceNr < long.MaxValue && maxSequenceNr > 0)
            {
                sb.Append(@" AND sequence_nr <= @sequence_nr");
                sqlCommand.Parameters.Add(new MySqlParameter("@sequence_nr", MySqlDbType.Int64)
                {
                    Value = maxSequenceNr
                });
            }

            if (maxTimestamp > DateTime.MinValue && maxTimestamp < DateTime.MaxValue)
            {
                sb.Append(" AND created_at <= @created_at");
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at", MySqlDbType.Int64)
                {
                    Value = maxTimestamp.Ticks
                });
            }

            sqlCommand.CommandText = sb.ToString();

            return sqlCommand;
        }

        public DbCommand InsertSnapshot(SnapshotEntry entry)
        {
            var sqlCommand = new MySqlCommand(_insertSql)
            {
                Parameters =
                {
                    new MySqlParameter("@persistence_id", MySqlDbType.VarChar, entry.PersistenceId.Length) { Value = entry.PersistenceId },
                    new MySqlParameter("@sequence_nr", MySqlDbType.Int64) { Value = entry.SequenceNr },
                    new MySqlParameter("@created_at", MySqlDbType.Int64) { Value = entry.Timestamp.Ticks },
                    new MySqlParameter("@manifest", MySqlDbType.VarChar, entry.SnapshotType.Length) { Value = entry.SnapshotType },
                    new MySqlParameter("@snapshot", MySqlDbType.Blob, entry.Snapshot.Length) { Value = entry.Snapshot }
                }
            };

            return sqlCommand;
        }

        public DbCommand SelectSnapshot(string persistenceId, long maxSequenceNr, DateTime maxTimestamp)
        {
            var sqlCommand = new MySqlCommand();
            sqlCommand.Parameters.Add(new MySqlParameter("@persistence_id", MySqlDbType.VarChar, persistenceId.Length)
            {
                Value = persistenceId
            });

            var sb = new StringBuilder(_selectSql);
            if (maxSequenceNr > 0 && maxSequenceNr < long.MaxValue)
            {
                sb.Append(" AND sequence_nr <= @sequence_nr ");
                sqlCommand.Parameters.Add(new MySqlParameter("@sequence_nr", MySqlDbType.Int64)
                {
                    Value = maxSequenceNr
                });
            }

            if (maxTimestamp > DateTime.MinValue && maxTimestamp < DateTime.MaxValue)
            {
                sb.Append(@" AND created_at <= @created_at");
                sqlCommand.Parameters.Add(new MySqlParameter("@created_at", MySqlDbType.Int64)
                {
                    Value = maxTimestamp.Ticks
                });
            }

            sb.Append(" ORDER BY sequence_nr DESC");
            sqlCommand.CommandText = sb.ToString();
            return sqlCommand;
        }
    }
}