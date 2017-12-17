using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    public struct LEDDto
    {
        public byte LED;
    }

    [BoardConfig(Name = "Quokka")]
    public static class LEDTest
    {
        public static async Task Aggregator(
            FPGA.OutputSignal<bool> LED1,
            FPGA.OutputSignal<bool> LED2,
            FPGA.OutputSignal<bool> LED3,
            FPGA.OutputSignal<bool> LED4,
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            bool internalLED1 = false, internalLED2 = false, internalLED3 = false, internalLED4 = false;
            FPGA.Config.Link(internalLED1, LED1);
            FPGA.Config.Link(internalLED2, LED2);
            FPGA.Config.Link(internalLED3, LED3);
            FPGA.Config.Link(internalLED4, LED4);

            Action statusHandler = () =>
            {
                Controllers.LEDDto data = new Controllers.LEDDto(); // TODO: support var
                data.LED = (byte)(( (internalLED4 ? 1 : 0) << 3) | ( (internalLED3 ? 1 : 0) << 2) | ( (internalLED2 ? 1 : 0) << 1 ) | (internalLED1 ? 1 : 0));

                Drivers.JSON.SerializeToUART<Controllers.LEDDto>(data, TXD); // TODO: autodetect generic type
            };

            FPGA.Config.OnTimer(TimeSpan.FromSeconds(1), statusHandler);

            FPGA.Signal<bool> commandDeserialized = new FPGA.Signal<bool>();
            Controllers.LEDDto command = new Controllers.LEDDto(); // TODO: autodetect type
            Drivers.JSON.DeserializeFromUART<Controllers.LEDDto>(command, RXD, commandDeserialized); // TODO: autodetect generic type

            Action receiverHandler = () =>
            {
                internalLED1 = (command.LED & 1) > 0; //FPGA.Config.Bit(command.LED, 0); // TODO: support struct access
                internalLED2 = (command.LED & 2) > 0;
                internalLED3 = (command.LED & 4) > 0;
                internalLED4 = (command.LED & 8) > 0;
            };

            FPGA.Config.OnSignal(commandDeserialized, receiverHandler);
        }
    }
}
