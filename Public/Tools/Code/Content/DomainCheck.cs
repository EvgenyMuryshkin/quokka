namespace Quokka.Public.Content
{
    public static class DomainCheck
    {
        public static bool IsAllowed(eContentDomain allowed, eContentDomain requested)
        {
            switch(allowed)
            {
                case eContentDomain.Public:
                    if (requested == eContentDomain.Public)
                        return true;
                    break;
                case eContentDomain.Trusted:
                    if (requested == eContentDomain.Public || requested == eContentDomain.Trusted)
                        return true;
                    break;
                case eContentDomain.Private:
                    return true;
            }

            return false;
        }
    }
}
