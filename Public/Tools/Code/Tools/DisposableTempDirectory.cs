using System;
using System.IO;

namespace Quokka.Public.Tools
{
    public class DisposableTempDirectory : IDisposable
    {
        public string DirectoryPath { get; private set; }

        public DisposableTempDirectory()
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), "XBD", Guid.NewGuid().ToString());

            Directory.CreateDirectory(DirectoryPath);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(DirectoryPath, true);
            }
            catch(Exception ex)
            {
                Exceptions.RethrowSystemException(ex);
                // this is not critical, ignore it
            }
        }
    }
}
