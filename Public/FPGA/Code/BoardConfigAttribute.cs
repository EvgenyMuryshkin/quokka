using System;

namespace FPGA.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class BoardConfigAttribute: Attribute
    {
        public string Name { get; set; }
        public int ClockFrequency { get; set; }
    }
}
