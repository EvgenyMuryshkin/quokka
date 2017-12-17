using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name="Sim")]
    [BoardConfig(Name="Quokka")]
    public static class DACTest
    {
        public static async Task Aggregator(
            FPGA.OutputSignal<bool> LED1,
            FPGA.OutputSignal<bool> DAC1NCS,
            FPGA.OutputSignal<bool> DAC1SCK,
            FPGA.OutputSignal<bool> DAC1SDI
            )
        {
            bool trigger = true;
            ushort channel = 0;
            sbyte channelDelta = 1;

            // Func can be used to create reusable data point for shifted operation
            // Func<ushort> dacInput = () => (ushort)(channel << 6);

            Action handler = () =>
            {
                channel = 0;
                channelDelta = 1;

                while(true)
                {
                    FPGA.Runtime.Delay(TimeSpan.FromMilliseconds(100));

                    Drivers.MCP49X2.Write((ushort)(channel << 6), ushort.MaxValue, DAC1NCS, DAC1SCK, DAC1SDI);

                    channel = (ushort)(channel + channelDelta);

                    switch (channel)
                    {
                        case ushort.MaxValue:
                            channelDelta = -1;
                            break;
                        case ushort.MinValue:
                            channelDelta = 1;
                            break;
                    }
                }
            };

            FPGA.Config.OnSignal(trigger, handler);

            Drivers.IsAlive.Blink(LED1);
        }
    }
}