using System;
using System.IO;
using System.Reflection;

namespace Quokka.Public.Tools
{
    public class Exceptions
    {
        public static void RethrowSystemException(Exception ex)
        {
            switch(ex)
            {
                case FileNotFoundException fnfe:
                case ReflectionTypeLoadException rtle:
                case FileLoadException fle:
                    throw ex;
                case AggregateException ae:
                    RethrowSystemException(ae.InnerException);
                    break;
                default:
                    break;
            }
        }
    }
}
