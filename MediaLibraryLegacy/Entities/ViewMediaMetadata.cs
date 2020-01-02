using System;

namespace MediaLibraryLegacy
{
    public struct ViewMediaMetadata
    {
        public string YID { get; set; }
        public string Title { get; set; }
        public Uri ThumbUri { get; set; }
        public string Quality { get; set; }
        public string MediaType { get; set; }
        public long Size { get; set; }
    }
}
