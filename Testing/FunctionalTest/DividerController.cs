﻿using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name = "Quokka")]
    public static class Math_Divider
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            DTOs.DividerRequest request = new DTOs.DividerRequest();
            FPGA.Signal<bool> deserialized = new FPGA.Signal<bool>();

            Drivers.JSON.DeserializeFromUART<DTOs.DividerRequest>(request, RXD, deserialized);

            Action processingHandler = () =>
            {
                ulong result, remainder;
                SequentialMath.Divider.Unsigned<ulong>(request.Numerator, request.Denominator, out result, out remainder);
                DTOs.DividerResponse response = new DTOs.DividerResponse();
                response.Result = result;
                response.Remainder = remainder;
                Drivers.JSON.SerializeToUART<DTOs.DividerResponse>(response, TXD);
            };

            FPGA.Config.OnSignal(deserialized, processingHandler);
        }
    }
}
