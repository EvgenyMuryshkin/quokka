using FPGA.Attributes;
using Quokka.Public.Snapshots;
using System;
using System.Collections.Generic;

namespace Quokka.Public.Transformation
{
    public class TransformationRequest
    {
        public Guid CorrelationId { get; set; } = Guid.NewGuid();

        public string License { get; set; }

        public List<SourceFileSnapshot> Sources { get; set; } = new List<SourceFileSnapshot>();

        public List<BoardConfigAttribute> Configurations { get; set; } = new List<BoardConfigAttribute>();

        public List<string> ControllerNames { get; set; }
    }
}
