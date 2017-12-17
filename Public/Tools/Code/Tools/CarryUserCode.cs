using Quokka.Public.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quokka.Public.Tools
{
    public class CarryUserCode
    {
        private readonly ILogStream _logStream;

        public CarryUserCode(ILogStream logStream)
        {
            _logStream = logStream;
        }

        string formatBegin(string pBegin)
        {
            return string.Format("[BEGIN USER {0}]", pBegin);
        }

        public string Transform(string transformed, string existing)
        {
            Dictionary<String, List<String>> mapUserData = new Dictionary<string, List<string>>();
            var userDataTypes = new List<string>() { "PORTS", "SIGNALS", "ARCHITECTURE" };
            List<String> listSourceLines = new List<String>(transformed.Split(new string[] { "\r\n" }, StringSplitOptions.None));
            List<String> listExistingSourceLines = new List<String>(existing.Split(new string[] { "\r\n" }, StringSplitOptions.None));

            /*
            {
                var beginMapSignature = "[BEGIN USER MAP FOR ";
                var endMapSignature = "[END USER MAP FOR ";

                Func<string, string> fetchEntityName =
                    (s) =>
                    {
                        return
                            s
                            .Replace("-", "") // remove VHDL comments
                            .Trim() // trim spaces
                            .Replace(beginMapSignature, "") // cut signature
                            .Replace("]", "") // remove trailing bracket
                            .Trim(); // trim spaces
                    };

                Func<List<String>, List<string>> fetchEntityMap =
                    (codeLines) =>
                    {
                        return codeLines
                               .Where(l => l.Contains(beginMapSignature))
                               .Select(
                                       l => fetchEntityName(l)
                                           ).Where(l => !String.IsNullOrWhiteSpace(l))
                        .ToList();
                    };

                var userEntityMaps = fetchEntityMap(listSourceLines);
                userDataTypes.AddRange(userEntityMaps.Select(t => string.Format("MAP FOR {0}", t)));

                var userExistingEntityMaps = new List<string>();

                int state = 0;
                string lastEntityMap = null;
                listExistingSourceLines.ForEach( 
                    s =>
                    {
                        if( s.Contains( beginMapSignature ))
                        {
                            if( 0 == state )
                            {
                                state = 1;
                                lastEntityMap = fetchEntityName(s);
                            }
                            return;
                        }

                        if( s.Contains(endMapSignature))
                        {
                            if( state == 2 )
                            {
                                userExistingEntityMaps.Add(lastEntityMap);
                                state = 0;
                            }

                            return;
                        }

                        if( state == 1)
                        {
                            state = 2;
                            return;
                        }
                    }
                    );

                var missingMappings = userExistingEntityMaps.Where(l => !userEntityMaps.Contains(l));

                if (missingMappings.Any())
                {
                    throw new Exception("Missing mappings: " + string.Join(",", missingMappings));
                }
            }
            */
            try
            {
                Func<string, List<String>> fetchLines =
                    (s) =>
                    {
                        var begin = formatBegin(s);
                        var end = string.Format("[END USER {0}]", s);

                        return
                            listExistingSourceLines
                            .SkipWhile((l) => !l.Contains(begin))
                            .Skip(1)
                            .TakeWhile((l) => !l.Contains(end))
                            .ToList();
                    };

                foreach (var pType in userDataTypes)
                    mapUserData[pType] = fetchLines(pType);
            }
            catch(Exception ex)
            {
                Exceptions.RethrowSystemException(ex);

                // do nothing here
                _logStream.Log(new Exception("Failed to carry over custom code", ex));
            }

            StringBuilder result = new StringBuilder();

            foreach( var pType in mapUserData )
            {
                var pBegin = formatBegin(pType.Key);
                var pIndex = listSourceLines.FindIndex(0, (s) => s.Contains(pBegin));
                if( -1 != pIndex)
                {
                    listSourceLines =
                        listSourceLines
                        .Take(pIndex + 1)
                        .Concat(pType.Value)
                        .Concat(listSourceLines.Skip(pIndex + 1))
                        .ToList();
                }
            }

            listSourceLines.ForEach(s => result.AppendLine(s));

            return result.ToString();
        }
    }
}
