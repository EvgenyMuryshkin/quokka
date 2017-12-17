using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class Runtime_DictionaryController
    {
        public static void Lookpup(byte key, out byte value)
        {
            FPGA.Collections.ReadOnlyDictionary<byte, byte> items = new FPGA.Collections.ReadOnlyDictionary<byte, byte>()
            {
                { 0, 1 },
                { 1, 2 },
                { 2, 4 },
                { 3, 10 },
                { 4, 15 }
            };

            value = items[key];
        }

        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            // TODO: support for const trigger
            bool trigger = true;


            Action handler = () =>
            {
                byte data = 0;
                Drivers.UART.Read(RXD, out data);

                byte result = 0;
                Controllers.Runtime_DictionaryController.Lookpup(data, out result);

                Drivers.UART.Write(result, TXD);
            };
            FPGA.Config.OnSignal(trigger, handler);
        }
    }
}
