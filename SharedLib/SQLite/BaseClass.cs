using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib.SQLite
{

    public class BaseClass
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Type { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }



}
