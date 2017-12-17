using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace DTO
{
    public struct Obj1
    {
        public byte code;
        public uint f1;
        public uint f2, f3;
    }
}


namespace Controllers
{
    [BoardConfig(Name = "Sim")]
    public static class KeypadTest
    {
        public static async Task Keypad(
            FPGA.OutputSignal<bool> K7,
            FPGA.OutputSignal<bool> K6,
            FPGA.OutputSignal<bool> K5,
            FPGA.OutputSignal<bool> K4,
            FPGA.InputSignal<bool> K3,
            FPGA.InputSignal<bool> K2,
            FPGA.InputSignal<bool> K1,
            FPGA.InputSignal<bool> K0,
            FPGA.OutputSignal<byte> data)
        {
            byte code = 0;
            FPGA.Config.Link(code, data);

            bool trigger = true;
            Action handler = () =>
            {
                Drivers.Keypad4x4.ReadCode(K7, K6, K5, K4, K3, K2, K1, K0, out code);
            };

            FPGA.Config.OnSignal(trigger, handler);
            
        }
    }

    [BoardConfig(Name = "Sim")]
    public static class WaitTest
    {
        public static async Task Test(
            FPGA.InputSignal<bool> InSignal,
            FPGA.OutputSignal<bool> OutSignal)
        {
            Action handler = () =>
            {
                OutSignal = true;
                FPGA.Runtime.Delay(TimeSpan.FromMilliseconds(1));
                OutSignal = true;
            };

            FPGA.Config.OnSignal(InSignal, handler);
        }
    }

    // TODO: Sim condifuration does not compile, raise error that timer cannot be met on given frequency
    [BoardConfig(Name = "Quokka")]
    [BoardConfig(Name = "Sim", ClockFrequency=50000000)]
    public static class SUTest
    {
        public static async Task Aggregator(
            FPGA.OutputSignal<bool> LED1,
            FPGA.OutputSignal<bool> LED2,
            FPGA.OutputSignal<bool> LED3,
            FPGA.OutputSignal<bool> LED4,
            FPGA.OutputSignal<bool> TXD,
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> K7,
            FPGA.OutputSignal<bool> K6,
            FPGA.OutputSignal<bool> K5,
            FPGA.OutputSignal<bool> K4,
            FPGA.InputSignal<bool> K3,
            FPGA.InputSignal<bool> K2,
            FPGA.InputSignal<bool> K1,
            FPGA.InputSignal<bool> K0,
            FPGA.OutputSignal<bool> Bank1,
            FPGA.OutputSignal<bool> Bank2
            )
        {
            //FPGA.Config.Link(RXD, LED3);
            FPGA.Config.Default(out LED1, false);
            FPGA.Config.Default(out LED2, false);
            FPGA.Config.Default(out LED3, false);
            FPGA.Config.Default(out LED4, false);

            // TODO: create dedicated Input() and Output() macros
            FPGA.Config.Default(out Bank1, true); // output
            FPGA.Config.Default(out Bank2, false); // input
            FPGA.Config.Suppress("W0002", Bank1, Bank2, LED1, LED2, LED3, LED4);

            bool internalLED2 = false;
            FPGA.Config.Link(internalLED2, LED4);

            byte uartData = 0;

            Action deserializedHandler = () =>
            {
                int c = 0;

                while (true)
                {
                    Drivers.UART.Read(RXD, out uartData);

                    c++;

                    if(c == 10000)
                    {
                        c = 0;
                        //FPGA.Runtime.Delay(TimeSpan.FromSeconds(1));
                        internalLED2 = !internalLED2;
                    }

//                    if (data == 10 )
//                        internalLED2 = !internalLED2;
//                    FPGA.Runtime.Delay(TimeSpan.FromSeconds(1));

/*
                    if (data == 93)
                    {
                        internalLED2 = !internalLED2;
                    }*/
                }
            };

            bool trigger = true;
            FPGA.Config.OnSignal(trigger, deserializedHandler);

            /*
            DTO.Obj1 rcv = new DTO.Obj1();
            FPGA.Signal<bool> deserialized = new FPGA.Signal<bool>();

            Drivers.JSON.DeserializeFromUART<DTO.Obj1>(rcv, RXD, deserialized);
            Action deserializedHandler = () =>
            {
                internalLED2 = !internalLED2;
            };
            FPGA.Config.OnSignal(deserialized, deserializedHandler);
            */

            uint counter = 0;
            Action handler = () =>
            {
                DTO.Obj1 data = new DTO.Obj1();

                counter++;
                data.f1 = counter;
                data.f2 = uartData;
                data.f3 = FPGA.Runtime.StateOf(deserializedHandler);

                byte code = 0;
                Drivers.Keypad4x4.ReadASCIICode(K7, K6, K5, K4, K3, K2, K1, K0, out code);

                data.code = code;

                Drivers.JSON.SerializeToUART<DTO.Obj1>(data, TXD);
            };

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(100), handler);

            //Drivers.IsAlive.Blink(LED1);
        }
    }
}
