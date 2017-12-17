using Quokka.Public.Content;
using Quokka.Public.Logging;
using Quokka.Public.Snapshots;
using Quokka.Public.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Quokka.Public.Watchers
{
    public class CSharpProjectWatcher : IDisposable
    {
        private readonly ILogStream _logStream;
        Subject<ProjectFoldersSnapshot> _stream = new Subject<ProjectFoldersSnapshot>();
        Subject<FSEvent> _fsStream = new Subject<FSEvent>();
        IDisposable _fsSubscription;

        private string ProjectPath;

        private string ProjectFolder => Path.GetDirectoryName(ProjectPath);

        public IObservable<ProjectFoldersSnapshot> FoldersStream => _stream;
        List<FSWatcher> watchers = new List<FSWatcher>();

        public CSharpProjectWatcher(ILogStream logStream)
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
                    catch (Exception ex)
                    {
                        Exceptions.RethrowSystemException(ex);

                        _logStream.Log(ex);
                    }
                });
        }

        public void WatchProject(string projectPath)
        {
            ProjectPath = projectPath;

            _fsStream.OnNext(new FSChange(Path.GetDirectoryName(projectPath), projectPath));
        }

        void EventHandler(IEnumerable<FSEvent> list)
        {
            var projectFiles = list.Where(e => Path.GetExtension(e.FullPath) == CSharpProject.CSharpProjectExtension);

            if(projectFiles.Any())
            {
                DisposeWatchers();

                var projectReferences = CSharpProject.RecursiveCollectProjectReferences(ProjectPath);
                var projectFolders = projectReferences.Select(r => Path.GetDirectoryName(r));

                watchers = projectFolders.Select(p => new FSWatcher(_logStream, p)).ToList();
                watchers.ForEach(w => w.Subscribe(e => _fsStream.OnNext(e)));

                _logStream.WriteLine(eContentDomain.Public, "Projects were changed");

                _stream.OnNext(new ProjectFoldersSnapshot(projectFolders));
            }
        }

        void DisposeWatchers()
        {
            watchers.ForEach(w => w.Dispose());
            watchers.Clear();
        }

        public void Dispose()
        {
            DisposeWatchers();

            if (_fsSubscription != null)
            {
                _fsSubscription.Dispose();
            }
            _fsSubscription = null;
        }

    }
}
