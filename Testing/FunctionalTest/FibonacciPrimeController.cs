using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class Math_FibonacciPrime
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

                    for(uint counter = 0; counter < 100; counter++)
                    {
                        ulong fib = 0;
                        SequentialMath.Calculators.Fibonacci(counter, out fib);

                        if (fib > uint.MaxValue)
                            break;

                        bool isPrime = false;
                        SequentialMath.Calculators.IsPrime((uint)fib, out isPrime);

                        if (isPrime)
                        {
                            for (byte i = 0; i < 4; i++)
                            {
                                byte data = (byte)fib;
                                Drivers.UART.Write(data, TXD);
                                fib = fib >> 8;
                            }
                        }
                    }
                }
            };

            bool trigger = true;
            FPGA.Config.OnSignal(trigger, handler);
        }
    }
}
