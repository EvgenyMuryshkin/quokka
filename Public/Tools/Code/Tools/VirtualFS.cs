using Quokka.Public.Content;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Quokka.Public.Tools
{
    public class VirtualFS
    {
        private readonly eContentDomain _contentDomain;
        private readonly ConcurrentDictionary<string, string> _content = new ConcurrentDictionary<string, string>();

        public VirtualFS(IContentDomainProvider contentDomainProvider)
        {
            _contentDomain = contentDomainProvider.ContentDomain;
        }

        public void Add(eContentDomain domain, string name, string content)
        {
            if( DomainCheck.IsAllowed(_contentDomain, domain) )
            {
                _content.AddOrUpdate(name, content, (key,oldValue) => content);
            }
        }

        public void Add(eContentDomain domain, string name, Func<string> content)
        {
            if (DomainCheck.IsAllowed(_contentDomain, domain))
            {
                _content.AddOrUpdate(name, (key) => content(), (key, oldValue) => content());
            }
        }

        public void Reset()
        {
            _content.Clear();
        }
        
        public Dictionary<string,string> ToDictionary()
        {
            return new Dictionary<string, string>(_content);
        }
    }
}
