using CommandLineParser.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quokka.Public.Tools
{
    public class RuntimeConfiguration
    {
        internal HashSet<string> _controllersToTranslate = null;

        public RuntimeConfiguration()
        {

        }

        public static RuntimeConfiguration FromCommandLineArguments(string[] args)
        {
            var rc = new RuntimeConfiguration();

            // http://fclp.github.io/fluent-command-line-parser/
            var p = new FluentCommandLineParser();

            List<string> controllers = null;

            p.Setup<List<string>>('c', "controllers")
             .Callback(items => controllers = items);

            var result = p.Parse(args);

            if (result.HasErrors)
            {
                throw new AggregateException(result.ErrorText, result.Errors.Select(e => new Exception(e.ToString())));
            }

            rc._controllersToTranslate = controllers?
                .Select(c => c.ToLower())
                .ToHashSet();

            return rc;
        }

        public void SetControllers(IEnumerable<string> controllers)
        {
            _controllersToTranslate = controllers?.ToHashSet();
        }

        public IEnumerable<string> GetControllers => _controllersToTranslate;

        public bool ShouldTranslateController(string name)
        {
            if( _controllersToTranslate == null)
            {
                return true;
            }

            return _controllersToTranslate.Contains(name.ToLower());
        }
    }
}
