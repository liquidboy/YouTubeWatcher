using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouTubeWatcher.sqlite
{
    public partial class AppDatabase : SqliteDatabase
    {
        private static AppDatabase _database = null;

        public static AppDatabase CurrentInstance
        {
            get
            {
                if (_database == null)
                {
                    throw new Exception("db has not been initialized .....");
                }
                return _database;

            }
        }

        public static AppDatabase Current(string dbLocation)
        {
            AppDatabase result;
            lock (lockobj)
            {
                if (_database == null)
                {
                    _database = new AppDatabase(dbLocation);
                }
                result = _database;
            }
            return result;

        }


        public Dictionary<string, TableSameDatabase> Tables;

        private AppDatabase(string dbLocation) : base("xapp.db", dbLocation) { }

        public void Init()
        {
            Tables = new Dictionary<string, TableSameDatabase>();
            this.Connection.CreateTable<Table>();
            refreshTables();
        }

        public void Unload()
        {
            _database.Connection.Close();
            _database.Connection.Dispose();
            _database = null;
        }

        public void AddTable(string tableName, string userName)
        {
            var exists = DoesTableExist(tableName);

            if (exists) return;

            var newTable = new Table()
            {
                Name = tableName,
                Type = (int)eTableType.UserDefined,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow.ToUniversalTime()
            };

            this.Connection.Insert(newTable);
            try
            {
                refreshTables();
            }
            catch (Exception ex)
            {

            }

        }

        public bool DoesTableExist(string name)
        {
            var row = this.Connection.Query<Table>("SELECT * FROM 'Table' WHERE Name = ?", name);
            return row.Count() > 0;
        }

        private void refreshTables()
        {
            var tables = this.Connection.Query<Table>("SELECT * FROM 'Table'");

            foreach (var table in tables)
            {
                if (!Tables.ContainsKey(table.Name))
                {
                    TableSameDatabase tblDB = new TableSameDatabase(table.Name, this.Connection);
                    Tables.Add(table.Name, tblDB);
                }

            }
        }
    }

}
