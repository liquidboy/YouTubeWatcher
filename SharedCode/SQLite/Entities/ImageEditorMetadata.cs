using System;
using System.Collections.Generic;
using System.Text;

namespace SharedCode.SQLite
{
    public class ImageEditorMetadata : BaseEntity
    {
        public Guid MediaUid { get; set; }
        public DateTime DateStamp { get; set; }   
        public double TotalSeconds { get; set; }
        public int Number { get; set; }
    }
}
