using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCode.SQLite
{
    public class PlaylistMediaMetadata : BaseEntity
    {
        public Guid PlaylistUid { get; set; }
        public Guid MediaUid { get; set; }
        public DateTime DateStamp { get; set; }
    }
}
