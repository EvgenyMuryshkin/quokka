using System.Collections.Generic;

namespace Quokka.Public.Snapshots
{
    public class ProjectFoldersSnapshot : List<string>
    {
        public ProjectFoldersSnapshot()
        {

        }

        public ProjectFoldersSnapshot(IEnumerable<string> folders)
        {
            this.AddRange(folders);
        }
    }
}
