using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCode.SQLite
{
    public class PlaylistMetadata : BaseEntity
    {
        public string Title { get; set; }
        public DateTime DateStamp { get; set; }
    }
}
