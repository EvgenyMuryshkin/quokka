using System.Collections.Concurrent;
using System.Linq;

namespace Quokka.Public.Profiling
{
    public class ProfilerFactory : IProfilerFactory
    {
        ConcurrentDictionary<string, IProfiler> mapProfilers = new ConcurrentDictionary<string, IProfiler>();

        public IProfiler Default => Create("__general");

        public IProfiler Create(string name)
        {
            return mapProfilers.GetOrAdd(name, new Profiler(name));
        }

        public object Serialize()
        {
            var profilerLog = new
            {
                ProfilerData = mapProfilers.Values.OrderBy(p => p.Name).Select(p => p.Serialize()).ToList()
            };

            return profilerLog;
        }
    }
}
