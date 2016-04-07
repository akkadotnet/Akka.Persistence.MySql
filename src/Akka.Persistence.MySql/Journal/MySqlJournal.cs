using System.Data.Common;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.Sql.Common.Journal;
using MySql.Data.MySqlClient;

namespace Akka.Persistence.MySql.Journal
{
    public class MySqlJournalEngine : JournalDbEngine
    {
        public readonly MySqlJournalSettings MySqlJournalSettings;

        public MySqlJournalEngine(ActorSystem system)
            : base(system)
        {
            MySqlJournalSettings = new MySqlJournalSettings(system.Settings.Config.GetConfig(MySqlJournalSettings.JournalConfigPath));

            QueryBuilder = new MySqlJournalQueryBuilder(Settings.TableName, MySqlJournalSettings.MetadataTableName);
        }

        protected override string JournalConfigPath { get { return MySqlJournalSettings.JournalConfigPath; } }

        protected override DbConnection CreateDbConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        protected override void CopyParamsToCommand(DbCommand sqlCommand, JournalEntry entry)
        {
            sqlCommand.Parameters["@persistence_id"].Value = entry.PersistenceId;
            sqlCommand.Parameters["@sequence_nr"].Value = entry.SequenceNr;
            sqlCommand.Parameters["@is_deleted"].Value = entry.IsDeleted;
            sqlCommand.Parameters["@created_at"].Value = entry.Timestamp.Ticks;
            sqlCommand.Parameters["@manifest"].Value = entry.Manifest;
            sqlCommand.Parameters["@payload"].Value = entry.Payload;
        }
    }

    /// <summary>
    /// Persistent journal actor using MySql as persistence layer. It processes write requests
    /// one by one in synchronous manner, while reading results asynchronously.
    /// </summary>
    public class MySqlJournal : SqlJournal
    {
        public readonly MySqlPersistence Extension = MySqlPersistence.Get(Context.System);

        private readonly string _updateSequenceNrSql;

        public MySqlJournal() : base(new MySqlJournalEngine(Context.System))
        {
            string tableName = Extension.JournalSettings.MetadataTableName;

            _updateSequenceNrSql = @"INSERT INTO {0} (persistence_id, sequence_nr) VALUES(@persistence_id, @sequence_nr) ON DUPLICATE KEY UPDATE persistence_id = @persistence_id, sequence_nr = @sequence_nr".QuoteTable(tableName);
        }

        protected override async Task DeleteMessagesToAsync(string persistenceId, long toSequenceNr)
        {
            long highestSequenceNr = await DbEngine.ReadHighestSequenceNrAsync(persistenceId, 0);
            await base.DeleteMessagesToAsync(persistenceId, toSequenceNr);

            if (highestSequenceNr <= toSequenceNr)
            {
                await UpdateSequenceNr(persistenceId, highestSequenceNr);
            }
        }

        private async Task UpdateSequenceNr(string persistenceId, long toSequenceNr)
        {
            using (DbConnection connection = DbEngine.CreateDbConnection())
            {
                await connection.OpenAsync();
                using (DbCommand sqlCommand = new MySqlCommand(_updateSequenceNrSql))
                {
                    sqlCommand.Parameters.Add(new MySqlParameter("@persistence_id", MySqlDbType.VarChar, persistenceId.Length)
                    {
                        Value = persistenceId
                    });
                    sqlCommand.Parameters.Add(new MySqlParameter("@sequence_nr", MySqlDbType.Int64)
                    {
                        Value = toSequenceNr
                    });

                    sqlCommand.Connection = connection;
                    sqlCommand.CommandTimeout = (int)Extension.JournalSettings.ConnectionTimeout.TotalMilliseconds;
                    await sqlCommand.ExecuteNonQueryAsync();
                }
            }
        }
    }
}