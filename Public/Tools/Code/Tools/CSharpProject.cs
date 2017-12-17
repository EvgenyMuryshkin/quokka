using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Quokka.Public.Tools
{
    public class CSharpProject
    {
        private static HashSet<string> IgnoredFolders = new HashSet<string>(new[] { "obj", "bin" });
        public static string CSharpProjectExtension = ".csproj";
        public static HashSet<string> SupportedSourceFileTypes = new HashSet<string>( new[] { ".cs" } );

        public static string GetSingleProjectFromFolder(string folderPath)
        {
            var allProjects = Directory.EnumerateFiles(folderPath, $"*{CSharpProjectExtension}");
            switch (allProjects.Count())
            {
                case 0:
                    throw new Exception($"No projects found in '{folderPath}'");
                case 1:
                    return allProjects.First();
                default:
                    throw new Exception($"Multiple projects found in '{folderPath}'");
            }
        }

        public static void RecursiveCollectProjectReferences(string projectPath, HashSet<string> projects)
        {
            projectPath = FileTools.ToAbsolutePath(projectPath);
            var projectFolder = Path.GetDirectoryName(projectPath);

            if (!File.Exists(projectPath) || projects.Contains(projectPath))
                return;

            projects.Add(projectPath);

            var references = File.ReadAllLines(projectPath)
                .Where(l => l.Contains("ProjectReference"))
                .Select(l => Regex.Match(l, "\"(.*)\"").Groups[1].Value)
                .Where(l => new[] { "quokka.public.csproj", "fpga.csproj" }.All(p => !l.ToLower().Contains(p))); // HACK: exclude FPGA project

            foreach (var refPath in references)
            {
                var refProjectPath = Path.Combine(projectFolder, refPath);
                RecursiveCollectProjectReferences(refProjectPath, projects);
            }
        }

        public static IEnumerable<string> RecursiveCollectProjectReferences(string rootProjectPath)
        {
            var allFolders = new HashSet<string>();
            RecursiveCollectProjectReferences(rootProjectPath, allFolders);
            return allFolders;
        }

        public static void CollectFiles(string path, List<string> files)
        {
            files.AddRange(Directory.EnumerateFiles(path).Where(p => FileTools.IsFileMatched(p, SupportedSourceFileTypes)));

            Directory.EnumerateDirectories(path)
                .Where(d => !IgnoredFolders.Contains(Path.GetFileName(d)))
                .ForEach(d => CollectFiles(d, files));
        }
    }
}
