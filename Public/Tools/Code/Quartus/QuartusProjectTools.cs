using Quokka.Public.Content;
using Quokka.Public.Logging;
using Quokka.Public.Snapshots;
using Quokka.Public.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quokka.Public.Quartus
{
    public class QuartusProjectTools
    {
        private readonly ILogStream _logStream;

        public QuartusProjectTools(ILogStream logStream)
        {
            _logStream = logStream;
        }

        public void SaveCodeSnapshot(
            SourceCodeSnapshot codeSnapshot, 
            string project,
            string projectLocation,
            HashSet<string> configurations)
        {
            var generatedFilesLocation = Path.Combine(projectLocation, "generated", project);
            var data = new Dictionary<string, string>();

            FileTools.CreateDirectoryRecursive(generatedFilesLocation);

            var stage = new CarryUserCode(_logStream);

            foreach (var pair in codeSnapshot)
            {
                var fileName = Path.GetFileName(pair.FullPath);
                var existingFile = Path.Combine(generatedFilesLocation, fileName);

                if (File.Exists(existingFile))
                {
                    data[pair.FullPath] = stage.Transform(pair.Content, File.ReadAllText(existingFile));
                }
                else
                {
                    data[pair.FullPath] = pair.Content;
                }
            }

            foreach (var file in Directory.EnumerateFiles(generatedFilesLocation, "*.*"))
            {
                _logStream.WriteLine(eContentDomain.Public, ConsoleColor.DarkYellow, $"Deleting {Path.GetFileName(file)}");
                FileTools.DeleteFile(_logStream, file);
            }

            foreach (var pair in data.OrderBy(p => p.Key))
            {
                _logStream.WriteLine(eContentDomain.Public, ConsoleColor.Green, $"Writing {Path.GetFileName(pair.Key)}");
                File.WriteAllText(Path.Combine(generatedFilesLocation, pair.Key), pair.Value);
            }

            _logStream.WriteLine(eContentDomain.Public, $"Extracted {data.Count} files");

            SetupQSF(project, projectLocation, configurations);
        }

        public void SetupQSF(
            string project,
            string projectLocation,
            HashSet<string> configurations)
        {
            var generatedFilesLocation = Path.Combine(projectLocation, "generated", project);

            // drop all MIF files, they are carried over to next test for some reasons
            var dbLocation = Path.Combine(projectLocation, "generated", project);
            foreach(var mifFile in Directory.EnumerateFiles(dbLocation, "*.mif"))
            {
                FileTools.DeleteFile(_logStream, mifFile);
            }

            // simple implementation, all files that do not have _ in name are common,
            // target project file should match prefix of file
            var allVhdl = Directory.EnumerateFiles(generatedFilesLocation, "*.vhdl");
            var commonVhdl = allVhdl.Where(f => !Path.GetFileNameWithoutExtension(f).Contains("_"));

            foreach (var config in configurations)
            {
                var qsf = Path.Combine(projectLocation, $"{config}.qsf");
                if (!File.Exists(qsf))
                {
                    _logStream.WriteLine(eContentDomain.Public, $"Configuration [{config}.qsf] was not found");
                    continue;
                }

                var lines = File.ReadAllLines(qsf);

                var nonVHDLLines = lines.Where(l => !l.Contains("VHDL_FILE") || !l.Contains("generated/"));

                var prefixedVHDL = Directory.EnumerateFiles(generatedFilesLocation, "*.vhdl")
                    .Where(f => {
                        var name = Path.GetFileNameWithoutExtension(f);
                        // TODO: this will pick all shared files as well, create folder for all shared files
                        return name.Contains("_") && name.StartsWith($"{config}_");
                    });

                var newLines = new[]
                {
                    nonVHDLLines,
                    commonVhdl.Select( f => $"set_global_assignment -name VHDL_FILE generated/{project}/{Path.GetFileName(f)}" ),
                    prefixedVHDL.Select( f => $"set_global_assignment -name VHDL_FILE generated/{project}/{Path.GetFileName(f)}" )
                }.SelectMany(s => s);

                File.WriteAllLines(qsf, newLines);

                _logStream.WriteLine(eContentDomain.Public, $"Project [{config}] update");
            }
        }
    }
}
