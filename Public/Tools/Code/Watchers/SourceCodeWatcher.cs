using Quokka.Public.Content;
using Quokka.Public.Logging;
using Quokka.Public.Snapshots;
using Quokka.Public.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Quokka.Public.Watchers
{
    public class SourceCodeWatcher : IDisposable
    {
        private readonly ILogStream _logStream;

        Subject<SourceCodeSnapshot> _stream = new Subject<SourceCodeSnapshot>();
        Subject<FSEvent> _fsStream = new Subject<FSEvent>();
        IDisposable _fsSubscription;

        Dictionary<string, SourceFileSnapshot> _files = new Dictionary<string, SourceFileSnapshot>();

        public IObservable<SourceCodeSnapshot> SnaphostsStream => _stream;
        List<FSWatcher> watchers = new List<FSWatcher>();

        Dictionary<string, string> mapFolderToProject = new Dictionary<string, string>();

        public SourceCodeWatcher(ILogStream logStream)
        {
            _logStream = logStream;

            _fsSubscription = _fsStream
                .ObserveOn(Scheduler.Default)
                .Buffer(TimeSpan.FromMilliseconds(500))
                .Where(list => list.Any())
                .Subscribe(list => {
                    try
                    {
                        EventHandler(list);
                    }
                    catch(Exception ex)
                    {
                        Exceptions.RethrowSystemException(ex);

                        _logStream.Log(ex);
                    }
                });
        }

        void StreamInitialContent(IEnumerable<string> folders)
        {
            folders.ForEach(folder =>
            {
                var allFiles = new List<string>();

                CSharpProject.CollectFiles(folder, allFiles);

                allFiles.ForEach(file => _fsStream.OnNext(new FSChange(folder, file)));
            });
        }

        public void WatchProjectFolders(IEnumerable<string> folders)
        {
            DisposeWatchers();

            mapFolderToProject = folders.ToDictionary(k => k, v => CSharpProject.GetSingleProjectFromFolder(v));

            watchers = folders.Select(p => new FSWatcher(_logStream, p)).ToList();
            watchers.ForEach(w => w.Subscribe(e => _fsStream.OnNext(e)));

            StreamInitialContent(folders);
        }
        
        void EventHandler(IEnumerable<FSEvent> list)
        {
            var sourceFiles = list.Where(e => FileTools.IsFileMatched(e.FullPath, CSharpProject.SupportedSourceFileTypes));
            if (!sourceFiles.Any())
                return;

            foreach (var e in sourceFiles)
            {
                if (e is FSDelete)
                {
                    _files.Remove(e.FullPath);
                }

                if (e is FSChange)
                {
                    string content;
                    if (FileTools.TryReadAllText(e.FullPath, out content))
                    {
                        _files[e.FullPath] = new SourceFileSnapshot() {
                                ProjectName = mapFolderToProject[e.WatchPath],
                                FullPath = e.FullPath,
                                Content = content
                            };
                    }
                }
            }

            _logStream.WriteLine(eContentDomain.Public, "Codebase was changed");

            _stream.OnNext(new SourceCodeSnapshot(_files.Values));
        }

        void DisposeWatchers()
        {
            watchers.ForEach(w => w.Dispose());
            watchers.Clear();
        }

        public void Dispose()
        {
            DisposeWatchers();

            if(_fsSubscription != null)
            {
                _fsSubscription.Dispose();
            }
            _fsSubscription = null;
        }
    }
}
