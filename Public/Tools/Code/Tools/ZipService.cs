using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Quokka.Public.Tools
{
    public class ZipService
    {
        public Stream ZipStreams(Dictionary<string,Stream> inputStreams)
        {
            var outputStream = new MemoryStream();

            using (var zipFile = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
            {
                foreach (var pair in inputStreams)
                {
                    var entry = zipFile.CreateEntry(pair.Key);
                    using (var entryStream = entry.Open())
                    {
                        pair.Value.CopyTo(entryStream);
                    }
                }
            }

            outputStream.Seek(0, SeekOrigin.Begin);

            return outputStream;
        }

        public DisposableDictionary<string, Stream> UnzipStream(Stream inputStream)
        {
            var result = new DisposableDictionary<string, Stream>();

            using (var zipFile = new ZipArchive(inputStream, ZipArchiveMode.Read))
            {
                foreach (var zipEntry in zipFile.Entries)
                {
                    var stream = new MemoryStream();
                    zipEntry.Open().CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    result[zipEntry.Name] = stream;
                }
            }

            return result;
        }
    }
}
