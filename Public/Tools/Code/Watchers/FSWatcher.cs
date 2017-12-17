using Quokka.Public.Content;
using Quokka.Public.Logging;
using System;
using System.IO;
using System.Reactive.Subjects;

namespace Quokka.Public.Watchers
{
    public abstract class FSEvent
    {
        public string WatchPath { get; protected set; }
        public string FullPath { get; protected set; }
    }

    public class FSChange : FSEvent
    {
        public FSChange(string watchPath, string fullPath)
        {
            WatchPath = watchPath;
            FullPath = fullPath;
        }
    }

    public class FSDelete : FSEvent
    {
        public FSDelete(string watchPath, string fullPath)
        {
            WatchPath = watchPath;
            FullPath = fullPath;
        }
    }

    public class FSWatcher : IDisposable, IObservable<FSEvent>
    {
        private readonly ILogStream _logStream;

        FileSystemWatcher _watcher;
        Subject<FSEvent> _stream = new Subject<FSEvent>();

        public FSWatcher(ILogStream logStream, string path)
        {
            _logStream = logStream;

            _logStream.WriteLine(eContentDomain.Public, $"Watching {path}");
                
            _watcher = new FileSystemWatcher(path);
            _watcher.IncludeSubdirectories = true;

            _watcher.Renamed += (s, a) =>
            {
                _stream.OnNext(new FSChange(path, a.FullPath));
            };

            _watcher.Changed += (s, a) =>
            {
                _stream.OnNext(new FSChange(path, a.FullPath));
            };

            _watcher.Deleted += (s, a) =>
            {
                _stream.OnNext(new FSDelete(path, a.FullPath));
            };

            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _stream.Dispose();
        }

        public IDisposable Subscribe(IObserver<FSEvent> observer)
        {
            return _stream.Subscribe(observer);
        }
    }
}
