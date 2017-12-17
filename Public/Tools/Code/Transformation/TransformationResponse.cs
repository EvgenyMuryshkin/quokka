using Quokka.Public.Logging;
using Quokka.Public.Snapshots;
using System;
using System.Collections.Generic;

namespace Quokka.Public.Transformation
{
    public class TransformationResponse
    {
        public Guid CorrelationId { get; set; }

        public List<SourceFileSnapshot> Result { get; set; } = new List<SourceFileSnapshot>();

        public List<LogRecord> Logs { get; set; } = new List<LogRecord>();
    }
}
