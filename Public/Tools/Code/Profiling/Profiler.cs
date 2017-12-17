using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Quokka.Public.Profiling
{
    public class ProfilerCheckpoint
    {
        public ProfilerCheckpoint()
        {
            AbsoluteTime = DateTime.Now;
        }

        public string Tag { get; set; }
        public TimeSpan PreviousTime { get; set; }
        public TimeSpan CurrentTime { get; set; }
        public DateTime AbsoluteTime { get; set; }
    }

    public class Profiler : IProfiler
    {
        Stopwatch _stopwatch = new Stopwatch();
        List<ProfilerCheckpoint> _checkPoints = new List<ProfilerCheckpoint>();
        public string Name { get; set; }
        TimeSpan LastEventTime { get; set; }
        public Profiler(string name)
        {
            _stopwatch.Start();
            Name = name;
            LastEventTime = _stopwatch.Elapsed;
        }

        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

        public void Checkpoint(string tag)
        {
            lock(_checkPoints)
            {
                var currentTime = _stopwatch.Elapsed;
                _checkPoints.Add(new ProfilerCheckpoint() { Tag = tag, PreviousTime = LastEventTime, CurrentTime = currentTime });
                LastEventTime = currentTime;
            }
        }

        public object Serialize()
        {
            return new
            {
                Name = Name,
                Checkpoints = _checkPoints.Select(p =>
                new
                {
                    Tag = p.Tag,
                    AbsoluteTime = p.AbsoluteTime.ToString(),
                    RecordedAt = Convert.ToInt32(p.CurrentTime.TotalMilliseconds),
                    Duration = Convert.ToInt32((p.CurrentTime - p.PreviousTime).TotalMilliseconds)
                })
            };
        }
    }
}
