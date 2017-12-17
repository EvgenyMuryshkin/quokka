using System;
using Quokka.Public.Content;

namespace Quokka.Public.Logging
{
    public class ConsoleLogStream : BaseLogStream
    {
        public ConsoleLogStream(IContentDomainProvider contentDomainProvider) : base(contentDomainProvider)
        {
        }

        protected override void OnLog(LogRecord record)
        {
            foreach(var item in record.Items)
            {
                Console.BackgroundColor = item.Background;
                Console.ForegroundColor = item.Foreground;
                Console.Write(item.Message);
                if( item.NewLine )
                {
                    Console.WriteLine();
                }

                Console.ResetColor();
            }
        }
    }
}
