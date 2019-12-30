using System;
using System.Collections.Generic;
using System.Text;

namespace YouTubeWatcher.sqlite
{

    public abstract class BaseEntity
    {
        [SQLite.PrimaryKey]
        public Guid UniqueId { get; set; }
        public int _internalRowId;
    }


}
