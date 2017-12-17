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

            FPGA.Collections.ReadOnlyDictionary<ushort, sbyte> motor1Map = new FPGA.Collections.ReadOnlyDictionary<ushort, sbyte>()
            {
                { 0, -127 },
                { 500, -127 },
                { 1000, -127 },
                { 1500, -63 }, // 193
                { 2000, -12 },
                { 2500, 30 },
                { 3000, 85 },
                { 3500, 127 },
                { 4000, 127 },
                { 4500, 127 },
                { 5000, 127 },
                { 5500, 105 },
                { 6000, 34 },
            };

            FPGA.Collections.ReadOnlyDictionary<ushort, sbyte> motor2Map = new FPGA.Collections.ReadOnlyDictionary<ushort, sbyte>()
            {
                { 0, 0 },
                { 500, -46 }, //210
                { 1000, -106 },
                { 1500, -127 },
                { 2000, -127 },
                { 2500, -127 }, // 128
                { 3000, -127 },
                { 3500, -81 }, // 175
                { 4000, -25 },
                { 4500, 25 },
                { 5000, 74 },
                { 5500, 127 },
                { 6000, 127 },
            };
            
            Action handler = () =>
            {
                //motor1 = 0;// -127; 
                //motor2 = -128;
                //Controllers.TachoDriver.ToMotorValue(adc1, out motor1);
                //Controllers.TachoDriver.ToMotorValue(adc2, out motor2);
                
                while(currentRpm != rpm)
                {
                    if (currentRpm > rpm)
                    {
                        currentRpm-=500;
                    }
                    else if (currentRpm < rpm)
                    {
                        currentRpm+=500;
                    }

                    motor1 = motor1Map[currentRpm];
                    motor2 = motor2Map[currentRpm];

                    FPGA.Runtime.Delay(TimeSpan.FromMilliseconds(50));
                }
            };

            Action rmpHandler = () =>
            {
                if (rpm == 6000)
                    rpm = 0;
                else
                    rpm += 500;
            };

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(100), handler);

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(1000), rmpHandler);

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