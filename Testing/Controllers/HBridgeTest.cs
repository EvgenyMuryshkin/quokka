using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace Controllers
{
    [BoardConfig(Name="Sim")]
    [BoardConfig(Name="Quokka")]
    public static class HBridgeTest
    {
        public static async Task Aggregator(
            FPGA.OutputSignal<bool> Motor1Enabled,
            FPGA.OutputSignal<bool> Motor1Pin1,
            FPGA.OutputSignal<bool> Motor1Pin2,
            FPGA.OutputSignal<bool> LED1
            )
        {
            bool trigger = true;
            sbyte motor1 = 0, motor1Delta = 1;

            Action handler = () =>
            {
                motor1 = 0;
                motor1Delta = 1;

                while(true)
                {
                    FPGA.Runtime.Delay(TimeSpan.FromMilliseconds(100));
                    motor1 += motor1Delta;

                    switch (motor1)
                    {
                        case sbyte.MaxValue:
                            motor1Delta = -1;
                            break;
                        case sbyte.MinValue:
                            motor1Delta = 1;
                            break;
                    }
                }
            };

            FPGA.Config.OnSignal(trigger, handler);

            Drivers.IsAlive.Blink(LED1);

            Drivers.L298.SingleMotorDriver(motor1, Motor1Pin1, Motor1Pin2, Motor1Enabled);
        }
    }
}