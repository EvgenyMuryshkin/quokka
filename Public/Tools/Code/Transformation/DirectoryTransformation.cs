using Quokka.Public.Config;
using Quokka.Public.Content;
using Quokka.Public.Logging;
using Quokka.Public.Quartus;
using Quokka.Public.Snapshots;
using Quokka.Public.Tools;
using Quokka.Public.Watchers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Quokka.Public.Transformation
{
    public class DirectoryTransformation
    {
        private readonly ILogStream _logStream;
        private readonly QuokkaContainerScopeFactory _containerFactory;

        public DirectoryTransformation(
            ILogStream logStream,
            QuokkaContainerScopeFactory containerFactory)
        {
            _logStream = logStream;
            _containerFactory = containerFactory;
        }

        public void TransfromDirectory(string projectDirectory)
        {
            using (var scope = _containerFactory.CreateScope())
            {
                var logStream = scope.Resolve<ILogStream>();
                var projectTransformation = scope.Resolve<IQuokkaProjectTransformation>();

                var config = QuokkaConfigLoader.Load(projectDirectory);
                var projectPath = CSharpProject.GetSingleProjectFromFolder(projectDirectory);
                var projectReferences = CSharpProject.RecursiveCollectProjectReferences(projectPath);
                var projectFolders = projectReferences.Select(p => Path.GetDirectoryName(p));

                var codeFiles = projectFolders.Select(folder =>
                {
                    var allFiles = new List<string>();
                    CSharpProject.CollectFiles(folder, allFiles);

                    return allFiles.Select(f =>
                    {
                        var content = "";
                        FileTools.TryReadAllText(f, out content);

                        return new SourceFileSnapshot()
                        {
                            ProjectName = CSharpProject.GetSingleProjectFromFolder(folder),
                            FullPath = f,
                            Content = content
                        };
                    });
                }).SelectMany(s => s);


                var transformedCode = new SourceCodeSnapshot(
                    projectTransformation.Transform(
                        new TransformationRequest()
                        {
                            Sources = codeFiles.ToList(),
                            Configurations = config.Configurations
                        }).Result.Result);

                new QuartusProjectTools(logStream).SaveCodeSnapshot(
                    transformedCode,
                    config.Project,
                    config.ProjectLocation,
                    config.Configurations.Select(c => c.Name).ToHashSet());
            }
        }

        public void WatchDirectory(string projectDirectory)
        {
            var projectPath = CSharpProject.GetSingleProjectFromFolder(projectDirectory);

            _logStream.WriteLine(eContentDomain.Public, $"Watching {projectPath}");

            QuokkaConfigLoader.Validate(projectDirectory);

            _logStream.WriteLine(eContentDomain.Public, $"Configration validated");

            Subject<string> fileSteam = new Subject<string>();

            using (var sourceCodeWatcher = new SourceCodeWatcher(_logStream))
            {
                sourceCodeWatcher.SnaphostsStream
                    .Throttle(TimeSpan.FromMilliseconds(500))
                    .Subscribe(codeSnapshot =>
                    {
                        try
                        {
                            using (var scope = _containerFactory.CreateScope())
                            {
                                var logStream = scope.Resolve<ILogStream>();
                                var projectTransformation = scope.Resolve<IQuokkaProjectTransformation>();

                                codeSnapshot.SourceFiles.ForEach(p => logStream.WriteLine(eContentDomain.Public, p.FullPath));

                                var config = QuokkaConfigLoader.Load(projectDirectory);

                                logStream.WriteLine(eContentDomain.Public, $"{DateTime.Now}, Transforming");

                                var result = projectTransformation.Transform(new TransformationRequest()
                                {
                                    Sources = codeSnapshot.SourceFiles.ToList(),
                                    Configurations = config.Configurations
                                }).Result;

                                if (result.Result.Any())
                                {
                                    new QuartusProjectTools(logStream)
                                        .SaveCodeSnapshot(
                                            new SourceCodeSnapshot(result.Result),
                                            config.Project,
                                            config.ProjectLocation,
                                            config.Configurations.Select(c => c.Name).ToHashSet());
                                }
                                else
                                {
                                    logStream.WriteLine(eContentDomain.Public, $"Transformation failed");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Exceptions.RethrowSystemException(ex);

                            _logStream.Log(ex);
                        }
                    });

                using (var projectWatcher = new CSharpProjectWatcher(_logStream))
                {
                    projectWatcher.FoldersStream.Subscribe(s =>
                    {
                        s.ForEach(f => _logStream.WriteLine(eContentDomain.Public, f));

                        sourceCodeWatcher.WatchProjectFolders(s);
                    });

                    projectWatcher.WatchProject(projectPath);

                    Console.ReadLine();
                }
            }
        }
    }
}
