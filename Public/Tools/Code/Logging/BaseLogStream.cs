using Quokka.Public.Content;
using System.Linq;

namespace Quokka.Public.Logging
{
    public abstract class BaseLogStream : ILogStream
    {
        private readonly eContentDomain _contentDomain;
        public BaseLogStream(IContentDomainProvider contentDomainProvider)
        {
            _contentDomain = contentDomainProvider.ContentDomain;
        }

        protected abstract void OnLog(LogRecord record);

        public void Log(LogRecord record)
        {
            var filtered = record.ShallowCopy();

            filtered.Items = filtered.Items.Where(i => DomainCheck.IsAllowed(_contentDomain, i.Domain)).ToList();

            if( filtered.Items.Any())
            {
                OnLog(filtered);
            }
        }
    }
}
