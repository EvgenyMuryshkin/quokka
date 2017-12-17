using System;

namespace Drivers
{
    public static class JSON
    {
        public static void DeserializeFromUART<T>(
            T obj, 
            FPGA.InputSignal<bool> RXD, 
            FPGA.Signal<bool> deserialized) where T : new()
        {
            byte data = 0;
            FPGA.Config.JSONDeserializer(obj, data, deserialized);

            bool trigger = true;
            Action uartHandler = () =>
            {
                while(true)
                {
                    Drivers.UART.Read(RXD, out data);
                }
            };

            FPGA.Config.OnSignal(trigger, uartHandler);
        }

        public static void SerializeToUART<T>(T data, FPGA.OutputSignal<bool> TXD)
        {
            FPGA.Signal<bool> hasMoreData, triggerDequeue, dataDequeued;
            FPGA.Signal<byte> currentByte;

            FPGA.Config.JSONSerializer(
                data,
                out hasMoreData,
                out triggerDequeue,
                out dataDequeued,
                out currentByte);

            while (hasMoreData)
            {
                triggerDequeue = true;
                FPGA.Runtime.WaitForAllConditions(dataDequeued);
                Drivers.UART.Write(currentByte, TXD);
            }
        }
    }
}
