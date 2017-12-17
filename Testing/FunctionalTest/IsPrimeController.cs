using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class Math_IsPrime
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            DTOs.IsPrimeRequest request = new DTOs.IsPrimeRequest();
            FPGA.Signal<bool> deserialized = new FPGA.Signal<bool>();
            Drivers.JSON.DeserializeFromUART<DTOs.IsPrimeRequest>(request, RXD, deserialized);

            Action handler = () =>
            {
                bool result = false;
                uint source = request.value;
                // TODO: member access is not supported in function call
                SequentialMath.Calculators.IsPrime((uint)source, out result);

                DTOs.IsPrimeResponse response = new DTOs.IsPrimeResponse();
                response.value = request.value;
                response.result = (byte)((result == true) ? 1 : 0);

                Drivers.JSON.SerializeToUART<DTOs.IsPrimeResponse>(response, TXD);
            };

            FPGA.Config.OnSignal(deserialized, handler);
        }
    }
}
