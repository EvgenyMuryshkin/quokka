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
                    if (Path.GetExtension(fnfe.FileName).ToLower() == ".dll")
                        throw ex;
                    break;
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
