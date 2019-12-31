using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.YT
{
    public class MediaStream
    {
        public async Task<MemoryStream> prepareMediaStream(string path)
        {
            if (!File.Exists(path))
                return null;
            var memory = new MemoryStream(); // No need to dispose MemoryStream, GC will take care of this

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return memory;
        }
    }

}
