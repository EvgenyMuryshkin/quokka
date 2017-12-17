using Newtonsoft.Json;
using Quokka.Public.Tools;
using System;
using System.IO;

namespace Quokka.Public.Config
{
    public static class QuokkaConfigLoader
    {
        public static void Validate(string path)
        {
            QuokkaConfigLoader.Load(path);
        }

        public static QuokkaConfig Load(string path)
        {
            var configPath = File.Exists(path) ? path : Path.Combine(path, "quokka.json");

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(configPath, $"Config file quokka.json was not found");
            }

            var config = JsonConvert.DeserializeObject<QuokkaConfig>(File.ReadAllText(configPath));

            if( string.IsNullOrWhiteSpace(config.Project))
            {
                throw new Exception($"Property {nameof(config.Project)} is not specified in {configPath}");
            }

            if( config.Configurations == null || config.Configurations.Count == 0)
            {
                throw new Exception($"No configurations found in {configPath}");
            }

            var targetLocation = Path.IsPathRooted(config.ProjectLocation) ? config.ProjectLocation : Path.Combine(path, config.ProjectLocation);

            if (!Directory.Exists(targetLocation))
            {
                FileTools.CreateDirectoryRecursive(targetLocation);
            }

            config.ProjectLocation = targetLocation;

            return config;
        }
    }
}
