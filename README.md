# Akka.Persistence.MySql [![Build status](https://ci.appveyor.com/api/projects/status/6tiq7gs3kgsqj8xe/branch/master?svg=true)](https://ci.appveyor.com/project/ravengerUA/akka-persistence-mysql/branch/master)

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
