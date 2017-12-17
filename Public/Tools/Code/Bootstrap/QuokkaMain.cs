using Quokka.Public.Logging;
using Quokka.Public.Tools;
using Quokka.Public.Transformation;
using System;
using System.IO;

namespace Quokka.Public.Bootstrap
{
    public class QuokkaMain
    {
        private readonly ILogStream _logStream;
        private readonly DirectoryTransformation _directoryTransformation;

        public QuokkaMain(
            ILogStream logStream,
            DirectoryTransformation directoryTransformation
            )
        {
            _logStream = logStream;
            _directoryTransformation = directoryTransformation;
        }

        public void Run(string[] args)
        {
            try
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                _directoryTransformation.WatchDirectory(currentDirectory);
            }
            catch (Exception ex)
            {
                Exceptions.RethrowSystemException(ex);

                _logStream.Log(ex);

                Environment.Exit(1);
            }
        }
    }
}
