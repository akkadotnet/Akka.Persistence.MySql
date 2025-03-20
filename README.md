# Deprecation Notice: Akka.Persistence.MySql

> [!WARNING]
>
> All Akka.Persistence plugins based on the `Akka.Persistence.Sql.Common` package are now **deprecated**. We strongly recommend that you migrate to the new [`Akka.Persistence.Sql` or the `Akka.Persistence.Sql.Hosting`](https://github.com/akkadotnet/Akka.Persistence.Sql) package.

## Migration Resources

To assist with the migration, please refer to the following resources:

- **Migration Guide:**  
  Learn the necessary steps to upgrade by following the [Migration Guide](https://github.com/akkadotnet/Akka.Persistence.Sql/blob/dev/docs/articles/migration.md).

- **Migration Walkthrough:**  
  For a step-by-step walkthrough, check out our [Migration Walkthrough](https://github.com/akkadotnet/Akka.Persistence.Sql/blob/dev/docs/articles/migration-walkthrough.md).

- **Migration Guide Video:**  
  Watch the [Migration Guide Video](https://www.youtube.com/watch?v=gSmqUrVHPq8) on YouTube for a detailed explanation of the migration process.

---

# Akka.Persistence.MySql

Akka Persistence journal and snapshot store backed by MySql database.

**WARNING: Akka.Persistence.MySql plugin is still in beta and it's mechanics described below may be still subject to change**.

### Configuration

Both journal and snapshot store share the same configuration keys (however they resides in separate scopes, so they are defined distinctly for either journal or snapshot store):

Remember that connection string must be provided separately to Journal and Snapshot Store.

Example connection-string:

```hocon
akka.persistence.journal.mysql.connection-string = "Server=localhost;Port=3306;Database=somedb;Uid=someuser;Pwd=somepassword;"
akka.persistence.snapshot-store.mysql.connection-string = "Server=localhost;Port=3306;Database=somedb;Uid=someuser;Pwd=somepassword;"
```

All config options:

```hocon
akka.persistence{
  journal {
    plugin = "akka.persistence.journal.mysql"
    mysql {
      # qualified type name of the MySql persistence journal actor
      class = "Akka.Persistence.MySql.Journal.MySqlJournal, Akka.Persistence.MySql"

      # dispatcher used to drive journal actor
      plugin-dispatcher = "akka.actor.default-dispatcher"

      # connection string used for database access
      connection-string = ""

      # connection string name for .config file used when no connection string has been provided
      connection-string-name = ""

      # default MySql commands timeout
      connection-timeout = 30s

      # MySql table corresponding with persistent journal
      table-name = "event_journal"

      # should corresponding journal table be initialized automatically
      auto-initialize = off

      # timestamp provider used for generation of journal entries timestamps
      timestamp-provider = "Akka.Persistence.Sql.Common.Journal.DefaultTimestampProvider, Akka.Persistence.Sql.Common"

      # metadata table
      metadata-table-name = "metadata"

      # default serializer to use
      default-serializer = ""
    }
  }

  snapshot-store {
    plugin = "akka.persistence.snapshot-store.mysql"
    mysql {
      # qualified type name of the MySql persistence journal actor
      class = "Akka.Persistence.MySql.Snapshot.MySqlSnapshotStore, Akka.Persistence.MySql"

      # dispatcher used to drive journal actor
      plugin-dispatcher = "akka.actor.default-dispatcher"

      # connection string used for database access
      connection-string = ""

      # connection string name for .config file used when no connection string has been provided
      connection-string-name = ""

      # default MySql commands timeout
      connection-timeout = 30s

      # MySql table corresponding with persistent journal
      table-name = "snapshot_store"

      # should corresponding journal table be initialized automatically
      auto-initialize = off

      # default serializer to use
      default-serializer = ""
    }
  }
}
```
### Table Schema

SQL Server persistence plugin defines a default table schema used for journal, snapshot store and metadate table.

```SQL
CREATE TABLE IF NOT EXISTS journal (
  ordering BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  persistence_id VARCHAR(255) NOT NULL,
  sequence_nr BIGINT NOT NULL,
  is_deleted BIT NOT NULL,
  manifest VARCHAR(500) NOT NULL,
  created_at BIGINT NOT NULL,
  payload LONGBLOB NOT NULL,
  tags VARCHAR(2000) NULL,
  serializer_id INT,
  UNIQUE (persistence_id, sequence_nr),
  INDEX journal_sequence_nr_idx (sequence_nr),
  INDEX journal_created_at_idx (created_at)
);

CREATE TABLE IF NOT EXISTS snapshot (
  persistence_id VARCHAR(255) NOT NULL,
  sequence_nr BIGINT NOT NULL,
  created_at BIGINT NOT NULL,
  manifest VARCHAR(255) NOT NULL,
  snapshot LONGBLOB NOT NULL,
  serializer_id INT,
  PRIMARY KEY (persistence_id, sequence_nr),
  INDEX snapshot_sequence_nr_idx (sequence_nr),
  INDEX snapshot_created_at_idx (created_at)
);

CREATE TABLE IF NOT EXISTS metadata (
  persistence_id VARCHAR(255) NOT NULL,
  sequence_nr BIGINT NOT NULL,
  PRIMARY KEY (persistence_id, sequence_nr)
);
```

### Migration

All migration scripts should be tested before running in production!

#### From 1.0.0-beta to 1.0.0-beta2

```SQL
/* Update journal table */
ALTER TABLE journal drop primary key;
ALTER TABLE journal ADD COLUMN serializer_id INT;
ALTER TABLE journal ADD COLUMN ordering BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY;
CREATE UNIQUE INDEX journal_persistence_sequence_nr ON journal (persistence_id, sequence_nr);
ALTER TABLE journal CHANGE COLUMN tags tags VARCHAR(2000);
ALTER TABLE journal CHANGE COLUMN payload payload LONGBLOB;

/* Update snapshot table */
ALTER TABLE snapshot CHANGE COLUMN snapshot snapshot LONGBLOB;
ALTER TABLE snapshot ADD COLUMN serializer_id INT;
```
