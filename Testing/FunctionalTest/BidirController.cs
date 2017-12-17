using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class Runtime_BidirController
    {
        public static async Task Aggregator(
            FPGA.BidirSignal<bool> bidir1,
            FPGA.BidirSignal<bool> bidir2
            )
        {
            bool trigger = true;
            Action handler = () =>
            {
                bidir1 = false;
                bidir2 = false;

                FPGA.Runtime.SetBidirAsOutput(bidir1);
                FPGA.Runtime.SetBidirAsInput(bidir2);

                bidir1 = true;
                bidir1 = false;
                bidir1 = bidir2;

                FPGA.Runtime.SetBidirAsInput(bidir1);
                FPGA.Runtime.SetBidirAsInput(bidir2);
            };
            FPGA.Config.OnSignal(trigger, handler);
        }
    }
}
