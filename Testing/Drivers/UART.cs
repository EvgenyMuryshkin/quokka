using System;

namespace Drivers
{
    public static class UART
    {
        // module has one non-registered input bit, and one registered output byte
        public static void Read(FPGA.InputSignal<bool> RXD, out byte data)
        {
            // 115200, delay is expressed in nanosecods, baud rate is hardcoded for now
            const uint delay = 8680;
            const uint halfDelay = 4340;

            byte result = 0;

            // all combinational logic is expressed as delegates
            Func<bool> invertedRXD = () => !RXD;

            // wait for start bit
            FPGA.Runtime.WaitForAllConditions(invertedRXD);

            // wait for half bit time to allow some time shift errors
            FPGA.Runtime.Delay(halfDelay);

            // read 8 bits
            for (uint i = 0; i < 8; i++)
            {
                FPGA.Runtime.Delay(delay);

                // this is assign of combinational expression
                // evaluated and assigned during single clock cycle 
                result = (byte)((result >> 1) | (RXD << 7));
            }

            // stop bit
            FPGA.Runtime.Delay(delay);

            // assign result and complete method call
            data = result;
        }

        public static void Write(byte data, FPGA.OutputSignal<bool> TXD)
        {
            // 115200
            const uint delay = 8680;

            // default TXD is high
            bool internalTXD = true;

            // hardlink from register to output signal, it has to hold its value
            FPGA.Config.Link(internalTXD, TXD);

            byte stored = data;

            // start bit
            internalTXD = false;
            FPGA.Runtime.Delay(delay);

            // write data bits
            for (byte i = 0; i < 8; i++)
            {
                internalTXD = (stored & 1) > 0;
                FPGA.Runtime.Delay(delay);
                stored = (byte)(stored >> 1);
            }

            // stop bit
            internalTXD = true;
            FPGA.Runtime.Delay(delay);
            FPGA.Runtime.Delay(delay);
        }

        public static void Write1(byte data, out bool TXD)
        {
            // 115200
            const uint delay = 8680;

            byte stored = data;

            // start bit
            TXD = false;
            FPGA.Runtime.Delay(delay);

            // write data bits
            for (byte i = 0; i < 8; i++)
            {
                TXD = (stored & 1) > 0;
                FPGA.Runtime.Delay(delay);
                stored = (byte)(stored >> 1);
            }

            // stop bit
            TXD = true;
            FPGA.Runtime.Delay(delay);
            FPGA.Runtime.Delay(delay);
        }
    }
}
