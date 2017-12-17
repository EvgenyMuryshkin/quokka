using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace DTO
{
    public struct TachoValues
    {
        public ushort M1;
        public ushort M2;
    }
}

namespace Controllers
{
    [BoardConfig(Name="Quokka")]
    public static class TachoDriver
    {
        static void ToMotorValue(ushort adcValue, out sbyte motorValue)
        {
            // TODO: fix shifter logic
            ushort shifted = (ushort)((adcValue - 32767) >> 8);

            motorValue = (sbyte)(shifted);
        }

        public static async Task Aggregator(
            FPGA.OutputSignal<bool> LED1,

            // read test values from ADC
            FPGA.OutputSignal<bool> ADC1NCS,
            FPGA.OutputSignal<bool> ADC1SLCK,
            FPGA.OutputSignal<bool> ADC1DIN,
            FPGA.InputSignal<bool> ADC1DOUT,

            // motor1 pins
            FPGA.OutputSignal<bool> Motor1Enabled,
            FPGA.OutputSignal<bool> Motor1Pin1,
            FPGA.OutputSignal<bool> Motor1Pin2,

            // motor2 pins
            FPGA.OutputSignal<bool> Motor2Enabled,
            FPGA.OutputSignal<bool> Motor2Pin1,
            FPGA.OutputSignal<bool> Motor2Pin2,

            FPGA.OutputSignal<bool> TXD

            )
        {
            ushort adcChannel1Value = 32767, adcChannel2Value = 32767;

            Controllers.TachoDriver.ADCReader(
                ADC1NCS, ADC1SLCK, ADC1DIN, ADC1DOUT, 
                out adcChannel1Value, out adcChannel2Value);

            Controllers.TachoDriver.ADCToMotor(
                adcChannel1Value, 
                adcChannel2Value,
                Motor1Enabled, Motor1Pin1, Motor1Pin2,
                Motor2Enabled, Motor2Pin1, Motor2Pin2,
                TXD);

            Drivers.IsAlive.Blink(LED1);
        }

        static void ADCReader(
            FPGA.OutputSignal<bool> ADC1NCS,
            FPGA.OutputSignal<bool> ADC1SLCK,
            FPGA.OutputSignal<bool> ADC1DIN,
            FPGA.InputSignal<bool> ADC1DOUT,
            out ushort adc1,
            out ushort adc2
            )
        {
            ushort adcChannel1Value = 0, adcChannel2Value = 0;

            FPGA.Config.Link(adcChannel1Value, out adc1);
            FPGA.Config.Link(adcChannel2Value, out adc2);

            Action handler = () =>
            {
                Drivers.ADC102S021.Read(out adcChannel1Value, out adcChannel2Value, ADC1NCS, ADC1SLCK, ADC1DIN, ADC1DOUT);
            };

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(100), handler);
        }

        static void ADCToMotor(
            ushort adc1, 
            ushort adc2,

            // motor1 pins
            FPGA.OutputSignal<bool> Motor1Enabled,
            FPGA.OutputSignal<bool> Motor1Pin1,
            FPGA.OutputSignal<bool> Motor1Pin2,

            // motor2 pins
            FPGA.OutputSignal<bool> Motor2Enabled,
            FPGA.OutputSignal<bool> Motor2Pin1,
            FPGA.OutputSignal<bool> Motor2Pin2,

            FPGA.OutputSignal<bool> TXD
            )
        {
            sbyte motor1 = 0, motor2 = -127;
            ushort rpm = 0;
            ushort currentRpm = 0;

            // Peaks: 2150, 4300
            // 1075 rpm per sector
            
            Action handler = () =>
            {
                while(currentRpm != rpm)
                {
                    if (currentRpm > rpm)
                    {
                        currentRpm -= 500;
                        if( currentRpm < rpm || currentRpm > 6000 )
                        {
                            currentRpm = rpm;
                        }
                    }
                    else if (currentRpm < rpm)
                    {
                        currentRpm += 500;
                        if( currentRpm > rpm )
                        {
                            currentRpm = rpm;
                        }
                    }

                    if( currentRpm > 6000 )
                    {
                        currentRpm = 6000;
                    }

                    if( currentRpm <= 1075)
                    {
                        motor1 = -128;
                    }
                    else if (currentRpm <= 3225 )
                    {
                        uint part = (uint)((currentRpm - 1075) * 255);
                        uint result, remainder;

                        SequentialMath.Divider.Unsigned<uint>(part, 2150, out result, out remainder);
                        if (result > 255)
                            result = 255;

                        motor1 = (sbyte)(-128 + result);
                    }
                    else if ( currentRpm <= 5375 )
                    {
                        motor1 = 127;
                    }
                    else
                    {
                        // TODO: create identifiers scope for every block
                        uint part2 = (uint)((currentRpm - 5375) * 255);
                        uint result2, remainder2;

                        SequentialMath.Divider.Unsigned<uint>(part2, 2150, out result2, out remainder2);
                        if (result2 > 255)
                            result2 = 255;

                        motor1 = (sbyte)(127 - result2);
                    }

                    if (currentRpm <= 1075)
                    {
                        uint part3 = (uint)(currentRpm * 255);
                        uint result3, remainder3;

                        SequentialMath.Divider.Unsigned<uint>(part3, 2150, out result3, out remainder3);
                        if (result3 > 127)
                            result3 = 127;

                        motor2 = (sbyte)(0 - result3);
                    }
                    else if ( currentRpm <= 3225 )
                    {
                        motor2 = -128;
                    }
                    else if ( currentRpm <= 5375)
                    {
                        uint part4 = (uint)((currentRpm - 3225) * 255);
                        uint result4, remainder4;

                        SequentialMath.Divider.Unsigned<uint>(part4, 2150, out result4, out remainder4);
                        if (result4 > 255)
                            result4 = 255;

                        motor2 = (sbyte)(-128 + result4);
                    }
                    else
                    {
                        motor2 = 127;
                    }

                    FPGA.Runtime.Delay(TimeSpan.FromMilliseconds(50));
                }
            };

            Action rmpHandler = () =>
            {
                if (rpm == 6000)
                    rpm = 0;
                else
                    rpm += 5;
            };

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(100), handler);

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(10), rmpHandler);

            Drivers.L298.SingleMotorDriver(motor1, Motor1Pin1, Motor1Pin2, Motor1Enabled);
            Drivers.L298.SingleMotorDriver(motor2, Motor2Pin1, Motor2Pin2, Motor2Enabled);

            Controllers.TachoDriver.ReportTacho(motor1, motor2, TXD);
        }

        static void ReportTacho(
            sbyte motor1,
            sbyte motor2,
            FPGA.OutputSignal<bool> TXD)
        {
            Action reportHandler = () =>
            {
                DTO.TachoValues data = new DTO.TachoValues();
                data.M1 = (byte)motor1;
                data.M2 = (byte)motor2;
                Drivers.JSON.SerializeToUART<DTO.TachoValues>(data, TXD);
            };

            FPGA.Config.OnTimer(TimeSpan.FromSeconds(1), reportHandler);
        }
    }
}