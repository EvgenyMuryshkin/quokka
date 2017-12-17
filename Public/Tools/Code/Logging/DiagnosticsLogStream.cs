using System;
using System.Diagnostics;
using Quokka.Public.Content;

namespace Quokka.Public.Logging
{
    public class DiagnosticsLogStream : BaseLogStream
    {
        public DiagnosticsLogStream(IContentDomainProvider contentDomainProvider) : base(contentDomainProvider)
        {
        }

        protected override void OnLog(LogRecord record)
        {
            Debug.Write($"{record.TimeStamp}: ");

            foreach (var item in record.Items)
            {
                Debug.Write(item.Message);

                Console.Write(item.Message);
                if( item.NewLine )
                {
                    Debug.WriteLine("");
                }
            }
        }
    }
}
