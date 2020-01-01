using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCode.SQLite
{
    public class MediaMetadata : BaseEntity
    {
        public string Title { get; set; }
        public DateTime DateStamp { get; set; }
        public string YID { get; set; }
        public string ThumbUrl { get; set; }
        public string Quality { get; set; }
        public string MediaType { get; set; }
        public long Size { get; set; }
    }
}
