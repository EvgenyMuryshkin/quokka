using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class Math_Fibonacci
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            Action handler = () =>
            {
                while (true)
                {
                    byte start = 0;
                    Drivers.UART.Read(RXD, out start);

                    ulong result = 0;
                    SequentialMath.Calculators.Fibonacci(start, out result);

                    for(byte i = 0; i < 8; i++ )
                    {
                        byte data = (byte)result;
                        Drivers.UART.Write(data, TXD);
                        result = result >> 8;
                    }
                }
            };

            bool trigger = true;
            FPGA.Config.OnSignal(trigger, handler);
        }
    }
}
