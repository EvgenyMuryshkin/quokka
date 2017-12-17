using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class Triggers_ConstTrueTrigger
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            const bool trigger = true;
            Action handler = () =>
            {
                byte data = 0;
                Drivers.UART.Read(RXD, out data);

                Drivers.UART.Write(data, TXD);
            };

            FPGA.Config.OnSignal(trigger, handler);
        }
    }

    [BoardConfig(Name = "Quokka")]
    public static class Triggers_ConstFalseTrigger
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            const bool trigger = false;
            // this should never be triggered
            Action handler = () =>
            {
                byte data = 0;
                Drivers.UART.Read(RXD, out data);

                Drivers.UART.Write(data, TXD);
            };

            FPGA.Config.OnSignal(trigger, handler);
        }
    }


    [BoardConfig(Name = "Quokka")]
    public static class Triggers_Timer
    {
        public static async Task Aggregator(FPGA.OutputSignal<bool> LED)
        {
            bool state = false;
            FPGA.Config.Link(state, LED);

            Action blinkerHandler = () =>
            {
                state = !state;
            };

            FPGA.Config.OnTimer(TimeSpan.FromSeconds(1), blinkerHandler);
        }
    }

    // TODO: signal and register written basic tests
}
