using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCode.SQLite
{


    public abstract class SqliteDatabase
    {
        protected static object lockobj = new object();

        public SQLiteConnection Connection { get; private set; }

        public string Name { get; set; }
        public string Location { get; set; }

        public SqliteDatabase(string dbName, string dbLocation)
        {
            Name = dbName;
            //Location = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, dbName);
            Location = System.IO.Path.Combine(dbLocation, dbName);
            if (!DBManager.Current.DoesDatabaseExist(dbName))
            {
                this.Connection = new SQLiteConnection(Location);
                DBManager.Current.RegisterDatabase(dbName, this);
            }
        }

        public void ExecuteStatement(string sql)
        {

            if (this.Connection != null && !this.Connection.IsInTransaction)
            {

                //Statement statement = this._sqlitedb.PrepareStatement(sql);
                //statement.Execute();
                this.Connection.Execute(sql);

            }
        }

        public void Close()
        {
            Connection.Close();
            Connection = null;
            Name = string.Empty;
            System.IO.File.Delete(Location);
            Location = string.Empty;
        }
    }

}
