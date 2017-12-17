using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    // Test UART echo server example
    [BoardConfig(Name = "Sim")]
    [BoardConfig(Name = "Quokka")]
    public static class TestUART
    {
        // WriteEnable signals on rxd and txd lines are not required, so
        // native types cannot be used, so special types were introduced
        public static async Task Aggregator(FPGA.InputSignal<bool> RXD, FPGA.OutputSignal<bool> TXD)
        {
            const uint buffSize = 100;
            const uint period = 1000;

            // declare circular memory buffer for incoming data
            byte[] buff = new byte[100];
            byte readAddr = 0, writeAddr = 0;
            object guard = new object();

            // trigger signal, indicates that there is something in the buffer
            Func<bool> hasData = () => writeAddr != readAddr;

            // this is infinite receiving handler, stores all stuff in buffer
            Action receiverHandler = () =>
            {
                byte data = 0;
                while (true)
                {
                    Drivers.UART.Read(RXD, out data);
                    lock (guard)
                    {
                        buff[writeAddr] = data;
                        writeAddr++;

                        // normally, remainder operation should be used, 
                        // but it takes a lot of FPGA resources
                        if (writeAddr >= buffSize)
                            writeAddr = 0;
                    }
                }
            };

            // handler kicks in when there is something in the buffer
            Action transmitterHandler = () =>
            {
                byte data = 0;
                lock (guard)
                {
                    data = buff[readAddr];
                    readAddr++;
                    // normally, remainder operation should be used, 
                    // but it takes a lot of FPGA resources
                    if (readAddr >= buffSize)
                        readAddr = 0;
                }
                Drivers.UART.Write(data, TXD);
            };

            // hack, no OnStartup event on the board yet, coming soon
            // just start handler on timer event
            FPGA.Config.OnTimer(period, receiverHandler);

            // Trigger transmitter handler when data is in the buffer
            FPGA.Config.OnSignal(hasData, transmitterHandler);
        }
    }

}
