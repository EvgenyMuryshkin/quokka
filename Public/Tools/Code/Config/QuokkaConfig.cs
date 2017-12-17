using FPGA.Attributes;
using System.Collections.Generic;

namespace Quokka.Public.Config
{
    public class QuokkaConfig
    {
        public string Project { get; set; }
        public List<BoardConfigAttribute> Configurations { get; set; }
        public string ProjectLocation { get; set; }
    }
}
