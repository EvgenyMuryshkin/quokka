using Quokka.Public.Content;
using Quokka.Public.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Quokka.Public.Tools
{
    public class FileTools
    {
        public static void DeleteFile(ILogStream logStream, string filePath)
        {
            int counter = 3;
            while (counter-- > 0)
            {
                try
                {
                    File.Delete(filePath);
                    break;
                }
                catch (Exception ex)
                {
                    logStream.WriteLine(eContentDomain.Public, "Caught exception: ", ex.GetType().Name);

                    if (counter == 0)
                        throw;

                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                }
            }
        }

        public static void CreateDirectoryRecursive(string path)
        {
            if (Directory.Exists(path))
                return;

            var parent = Path.GetDirectoryName(path);
            if(parent != null )
            {
                CreateDirectoryRecursive(parent);
            }

            Directory.CreateDirectory(path);
        }

        public static string ToAbsolutePath(string path)
        {
            var parts = path.Split(new[] { '\\', '/' });

            var result = new List<string>();

            foreach (var part in parts)
            {
                if (part == "..")
                    result.RemoveAt(result.Count - 1);
                else
                    result.Add(part);
            }

            return string.Join("\\", result);
        }

        public static bool IsFileMatched(string path, IEnumerable<string> fileTypes)
        {
            return fileTypes.Contains(Path.GetExtension(path).ToLower());
        }

        public static bool TryReadAllText(string path, out string value)
        {
            while (true)
            {
                try
                {
                    value = File.ReadAllText(path);
                    return true;
                }
                catch(FileNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);

                    value = "";
                    return false;
                }
                catch (Exception ex)
                {
                    Exceptions.RethrowSystemException(ex);

                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Retrying in 1 second");
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                }
            }
        }
    }
}
