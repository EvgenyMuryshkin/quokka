using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name="Sim")]
    public static class TestController
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD, 
            FPGA.OutputSignal<bool> TXD,
            FPGA.InputSignal<bool> trigger,
            FPGA.InputSignal<long> f1,
            FPGA.InputSignal<long> f2,
            FPGA.OutputSignal<long> r
            )
        {
            sbyte sByte = -10;
            byte Byte = 10;
            long sOut = 0;
            bool internalTxd = true;
            FPGA.Config.Link(sOut, r);
            FPGA.Config.Link(internalTxd, TXD);

            Action handler = () =>
            {
                sOut = f1 + f2;
                sOut = f1 * f2;
                sOut = (f1 * f2) + 1;
                /*
                sOut = short.MaxValue;
                sOut = short.MinValue;
                sOut = (short)(Byte + sByte);
                sOut = (short)(Byte - sByte);
                sOut = (short)(Byte * sByte);
                sOut = (short)(Byte / sByte);
                sOut = (short)(sByte * Byte);
                sOut = (short)(sByte / Byte);
                */
            };

            FPGA.Config.OnSignal(trigger, handler);
        }
    }
}

/*
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drivers
{
    public static class RootController
    {
        public static void MeanValue(ushort[] buff, out ushort mean)
        {
            const uint buffSize = 6;
            // calc mean distance
            ushort sum = 0, data = 0;
            ushort min = 65535, max = 0;

            // find min and max values
            for (uint i = 0; i < buffSize; i++)
            {
                data = buff[i];
                if (data < min)
                    min = data;

                if (data > max)
                    max = data;
            }

            bool minExcluded = false, maxExcluded = false;

            for (uint j = 0; j < buffSize; j++)
            {
                data = buff[j];

                if (data == min && !minExcluded)
                {
                    minExcluded = true;
                    continue;
                }

                if (data == max && !maxExcluded)
                {
                    maxExcluded = true;
                    continue;
                }

                sum = (ushort)(sum + data);
            }

            mean = (ushort)(sum >> 2);
        }

        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD,
            FPGA.InputSignal<bool> Echo,
            FPGA.OutputSignal<bool> Trigger)
        {
            const uint detectionDistance = 10;
            const uint buffSize = 6;
            ushort last = 0;
            bool monitoring = false;
            byte addr = 0;
            ushort[] buff = new ushort[buffSize];
            bool buffFilled = false;

            byte dataToTransmit = 0;
            FPGA.Signal<bool> transmitted = false;

            Action transmitterHandler = () =>
            {
                Drivers.UART.Transmitter(dataToTransmit, TXD);
                transmitted = true;
            };

            Action measureHandler = () =>
            {
                ushort distance = 0;
                Drivers.HCSR04.Measure(Echo, Trigger, out distance);

                byte db1 = (byte)distance;
                byte db2 = (byte)(distance >> 8);

                dataToTransmit = db1;
                FPGA.Runtime.WaitForAllConditions(transmitted);

                dataToTransmit = db2;
                FPGA.Runtime.WaitForAllConditions(transmitted);

                buff[addr] = distance;
                addr++;

                if (addr >= buffSize)
                {
                    addr = 0;
                    buffFilled = true;
                }

                if (buffFilled)
                {
                    ushort mean = 0;
                    Drivers.RootController.MeanValue(buff, out mean);

                    byte b1 = (byte)mean;
                    byte b2 = (byte)(mean >> 8);

                    dataToTransmit = b1;
                    FPGA.Runtime.WaitForAllConditions(transmitted);
                    dataToTransmit = b2;
                    FPGA.Runtime.WaitForAllConditions(transmitted);


                    if (mean > last + detectionDistance)
                    {
                        if (monitoring)
                        {
                            dataToTransmit = 1;
                            FPGA.Runtime.WaitForAllConditions(transmitted);

                            monitoring = false;
                        }

                        last = mean;
                    }

                    if (mean < last)
                    {
                        last = mean;
                        monitoring = true;
                    }
                }

                dataToTransmit = 255;
                FPGA.Runtime.WaitForAllConditions(transmitted);
            };

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(300), measureHandler);
            //FPGA.Config.OnTimer(300000000, measureHandler);
            FPGA.Config.OnRegisterWritten(dataToTransmit, transmitterHandler);
        }
    }
}
*/