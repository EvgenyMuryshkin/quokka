using System;

namespace Drivers
{
    public static class IsAlive
    {
        public static void Blink(FPGA.OutputSignal<bool> LED)
        {
            bool internalAlive = false;
            FPGA.Config.Link(internalAlive, LED);

            Action aliveHandler = () =>
            {
                internalAlive = !internalAlive;
            };

            FPGA.Config.OnTimer(TimeSpan.FromSeconds(1), aliveHandler);
        }
    }
}
