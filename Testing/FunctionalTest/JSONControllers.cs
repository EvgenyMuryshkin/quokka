using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class JSON_Serializer
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            byte data = 0;

            Action processingHandler = () =>
            {
                DTOs.RoundTrip response = new DTOs.RoundTrip();
                response.b = data;
                Drivers.JSON.SerializeToUART<DTOs.RoundTrip>(response, TXD);
            };

            FPGA.Config.OnRegisterWritten(data, processingHandler);

            Action deserializeHandler = () =>
            {
                Drivers.UART.Read(RXD, out data);
            };

            bool trigger = true;
            FPGA.Config.OnSignal(trigger, deserializeHandler);
        }
    }

    [BoardConfig(Name = "Quokka")]
    public static class JSON_Deserializer
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            DTOs.RoundTrip request = new DTOs.RoundTrip();
            FPGA.Signal<bool> deserialized = new FPGA.Signal<bool>();
            Drivers.JSON.DeserializeFromUART<DTOs.RoundTrip>(request, RXD, deserialized);

            Action processingHandler = () =>
            {
                Drivers.UART.Write(request.b, TXD);
            };

            FPGA.Config.OnSignal(deserialized, processingHandler);
        }
    }
}
