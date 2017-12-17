namespace Quokka.Public.Content
{
    public class ContentDomainProvider : IContentDomainProvider
    {
        public ContentDomainProvider(eContentDomain contentDomain)
        {
            ContentDomain = contentDomain;
        }

        public eContentDomain ContentDomain { get; set; }
    }
}
