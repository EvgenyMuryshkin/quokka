using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Quokka.Public.Snapshots
{
    public class SourceCodeSnapshot : IEnumerable<SourceFileSnapshot>
    {
        public SourceCodeSnapshot()
        {
            SourceFiles = ImmutableList<SourceFileSnapshot>.Empty;
        }

        public SourceCodeSnapshot(ImmutableList<SourceFileSnapshot> files)
        {
            SourceFiles = files;
        }

        public SourceCodeSnapshot(IEnumerable<SourceFileSnapshot> files)
        {
            var builder = ImmutableList<SourceFileSnapshot>.Empty.ToBuilder();
            builder.AddRange(files);
            SourceFiles = builder.ToImmutable();
        }

        public readonly ImmutableList<SourceFileSnapshot> SourceFiles;

        public IEnumerator<SourceFileSnapshot> GetEnumerator()
        {
            return SourceFiles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SourceFiles.GetEnumerator();
        }
    }
}
