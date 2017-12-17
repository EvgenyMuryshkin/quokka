using Quokka.Public.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quokka.Public.Logging
{
    public interface ILogStream
    {
        void Log(LogRecord record);
    }

    public static class ILogStreamExtensions
    {
        public static IEnumerable<LogRecordItem> DefaultExceptionItems(Exception ex)
        {
            var exceptionItems = new List<LogRecordItem>
            {
                new LogRecordItem()
                {
                    Domain = eContentDomain.Public,
                    Foreground = ConsoleColor.Red,
                    Message = ex.Message,
                    NewLine = true
                },
                new LogRecordItem()
                {
                    Domain = eContentDomain.Trusted,
                    Foreground = ConsoleColor.Red,
                    Message = $"[{ex.GetType().Name}]: ",
                    NewLine = true
                },
                new LogRecordItem()
                {
                    Domain = eContentDomain.Private,
                    Foreground = ConsoleColor.DarkYellow,
                    Message = ex.StackTrace,
                    NewLine = true
                }
            };

            var inner = ex.InnerException != null ? DefaultExceptionItems(ex.InnerException) : Enumerable.Empty<LogRecordItem>();

            exceptionItems.AddRange(inner);

            return exceptionItems;
        }

        public static void Log(this ILogStream stream, Exception ex)
        {
            stream.Log(DefaultExceptionItems(ex));
        }

        public static void Log(this ILogStream stream, params LogRecordItem[] items)
        {
            stream.Log(new LogRecord() { Items = items.ToList() });
        }

        public static void Log(this ILogStream stream, params IEnumerable<LogRecordItem>[] items)
        {
            stream.Log(items.SelectMany(i => i).ToArray());
        }

        public static void WriteLine(this ILogStream stream, eContentDomain domain, string format = null, params object[] args)
        {
            stream.WriteLine(domain, ConsoleColor.White, format, args);
        }

        public static void WriteLine(this ILogStream stream, eContentDomain domain, ConsoleColor foreground, string format = null, params object[] args )
        {
            stream.Log(new LogRecord()
            {
                Items = new List<LogRecordItem>()
                {
                    new LogRecordItem()
                    {
                        Domain = domain,
                        Foreground = foreground,
                        Message = format != null ? string.Format(format, args) : null,
                        NewLine = true
                    }
                }
            });
        }
    }
}
