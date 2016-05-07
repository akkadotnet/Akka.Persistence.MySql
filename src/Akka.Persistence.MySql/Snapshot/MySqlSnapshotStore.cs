using System.Data.Common;
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
        private readonly MySqlPersistence _extension = MySqlPersistence.Get(Context.System);

        public MySqlSnapshotStore()
        {
            QueryBuilder = new MySqlSnapshotQueryBuilder(_extension.SnapshotSettings);
            QueryMapper = new MySqlSnapshotQueryMapper(Context.System.Serialization);
        }

        protected override DbConnection CreateDbConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        protected override SnapshotStoreSettings Settings { get { return _extension.SnapshotSettings; } }
    }
}