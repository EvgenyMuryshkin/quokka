using Quokka.Public.Content;
using System;
using System.Collections.Generic;

namespace Quokka.Public.Logging
{
    public class LogRecordItem
    {
        public eContentDomain Domain { get; set; } = eContentDomain.Private;
        public ConsoleColor Foreground { get; set; } = ConsoleColor.White;
        public ConsoleColor Background { get; set; } = ConsoleColor.Black;
        public string Message { get; set; }
        public bool NewLine { get; set; }
    }

    public class LogRecord
    {
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public List<LogRecordItem> Items { get; set; } = new List<LogRecordItem>();

        public LogRecord ShallowCopy()
        {
            return (LogRecord)this.MemberwiseClone();
        }
    }
}
