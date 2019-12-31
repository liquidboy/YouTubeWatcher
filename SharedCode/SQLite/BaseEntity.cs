using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCode.SQLite
{

    public abstract class BaseEntity
    {
        [global::SQLite.PrimaryKeyAttribute]
        public Guid UniqueId { get; set; }
        public int _internalRowId;
    }


}
