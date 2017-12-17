using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class Memory_ReadWrite
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {            
            Action mainHandler = () =>
            {
                byte data = 0;
                byte[] buff = new byte[1000];

                for( int i = 0; i < 1000; i++ )
                {
                    Drivers.UART.Read(RXD, out data);
                    buff[i] = data;
                }

                byte sum = 0;
                for (int i = 0; i < 1000; i++)
                {
                    data = buff[i];
                    sum += data;
                }

                Drivers.UART.Write(sum, TXD);
            };

            bool trigger = true;
            FPGA.Config.OnSignal(trigger, mainHandler);
        }
    }

    [BoardConfig(Name = "Quokka")]
    public static class Memory_ConstLength
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            Action mainHandler = () =>
            {
                const uint buffLength = 1000;
                byte data = 0;
                byte[] buff = new byte[buffLength];

                for (int i = 0; i < buff.Length; i++)
                {
                    Drivers.UART.Read(RXD, out data);
                    buff[i] = data;
                }

                byte sum = 0;
                for (int i = 0; i < buffLength; i++)
                {
                    data = buff[i];
                    sum += data;
                }

                Drivers.UART.Write(sum, TXD);
            };

            bool trigger = true;
            FPGA.Config.OnSignal(trigger, mainHandler);
        }
    }

    [BoardConfig(Name = "Quokka")]
    public static class Memory_InlineInit
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            bool internalTXD = true;
            FPGA.Config.Link(internalTXD, TXD);

            Action mainHandler = () =>
            {
                byte data = 0;
                byte[] buff = new byte[] { 0, 1, 2, 3, 4 };

                for (int i = 0; i < buff.Length; i++)
                {
                    Drivers.UART.Read(RXD, out data);
                    byte existing = 0;
                    existing = buff[i];
                    buff[i] = (byte)(data + existing);
                }

                for (int i = 0; i < buff.Length; i++)
                {
                    data = buff[i];
                    Drivers.UART.Write1(data, out internalTXD);
                }

                byte sum = 0;
                for (int i = 0; i < buff.Length; i++)
                {
                    data = buff[i];
                    sum += data;
                }

                Drivers.UART.Write1(sum, out internalTXD);
            };

            bool trigger = true;
            FPGA.Config.OnSignal(trigger, mainHandler);
        }
    }

    [BoardConfig(Name = "Quokka")]
    public static class Memory_Reinit
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            bool internalTXD = true;
            FPGA.Config.Link(internalTXD, TXD);

            Action mainHandler = () =>
            {
                byte data = 0;
                byte[] buff = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

                Drivers.UART.Read(RXD, out data);

                for (byte i = 0; i < buff.Length; i++)
                {
                    byte tmp = 0;
                    tmp = buff[i];
                    tmp += data;
                    buff[i] = tmp;

                    Drivers.UART.Write1(tmp, out internalTXD);
                }

                for (byte jjj = 0; jjj < buff.Length; jjj++)
                {
                    buff[jjj] = jjj;
                };
            };

            bool trigger = true;
            FPGA.Config.OnSignal(trigger, mainHandler);
        }
    }
}
