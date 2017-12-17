using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace DTO
{
    public struct ADC
    {
        public ushort IN1;
        public ushort IN2;
    }
}

namespace Controllers
{
    [BoardConfig(Name="Sim", ClockFrequency = 5000000)]
    [BoardConfig(Name="Quokka")]
    public static class ADCTest
    {
        static void ToMotorValue(ushort adcValue, out sbyte motorValue)
        {
            // TODO: fix shifter logic
            ushort shifted = (ushort)((adcValue - 32767) >> 8);

            sbyte nextValue = (sbyte)(shifted);
            if (nextValue > (sbyte.MaxValue - 10))
            {
                motorValue = sbyte.MaxValue;
            }
            else if (nextValue < sbyte.MinValue + 10)
            {
                motorValue = sbyte.MinValue;
            }
            else if (nextValue > 5 || nextValue < -5)
            {
                motorValue = (sbyte)(shifted);
            }
            else
            {
                motorValue = 0;
            }
        }

        public static async Task Aggregator(
            FPGA.OutputSignal<bool> LED1,
            FPGA.OutputSignal<bool> LED2,
            FPGA.OutputSignal<bool> LED3,
            FPGA.OutputSignal<bool> LED4,

            // send test values to DAC
            FPGA.OutputSignal<bool> DAC1NCS,
            FPGA.OutputSignal<bool> DAC1SCK,
            FPGA.OutputSignal<bool> DAC1SDI,

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

            FPGA.OutputSignal<bool> TXD,

            // keypad
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
            ushort adcChannel1Value = 32767, adcChannel2Value = 32767;
            byte keypadCode = 0;
            bool enableMotors = false;

            Controllers.ADCTest.ADCReader(ADC1NCS, ADC1SLCK, ADC1DIN, ADC1DOUT, out adcChannel1Value, out adcChannel2Value);

            Controllers.ADCTest.ADCToDAC(adcChannel1Value, adcChannel2Value, DAC1NCS, DAC1SCK, DAC1SDI);

            Controllers.ADCTest.ADCToMotor(
                enableMotors,
                adcChannel1Value, 
                adcChannel2Value,
                Motor1Enabled, Motor1Pin1, Motor1Pin2,
                Motor2Enabled, Motor2Pin1, Motor2Pin2);

            Controllers.ADCTest.KeypadToLEDs(
                LED1, LED2, LED3, LED4,
                K7, K6, K5, K4, K3, K2, K1, K0,
                Bank1, Bank2,
                out keypadCode);

            Controllers.ADCTest.ReportADC(adcChannel1Value, adcChannel2Value, TXD);

            Controllers.ADCTest.KeypadToMotor(keypadCode, out enableMotors);
        }

        static void KeypadToMotor(
            byte keypadCode,
            out bool enableMotors)
        {
            bool internalEnableMotors = false;
            FPGA.Config.Link(internalEnableMotors, out enableMotors);

            Action keypadHandler = () =>
            {
                switch (keypadCode)
                {
                    case 4:
                        internalEnableMotors = false;
                        break;
                    case 8:
                        internalEnableMotors = true;
                        break;
                }
            };
            FPGA.Config.OnRegisterWritten(keypadCode, keypadHandler);
        }

        static void ADCToDAC(
            ushort adc1,
            ushort adc2,
            FPGA.OutputSignal<bool> DAC1NCS,
            FPGA.OutputSignal<bool> DAC1SCK,
            FPGA.OutputSignal<bool> DAC1SDI
            )
        {
            Action handler = () =>
            {
                Drivers.MCP49X2.Write(adc1, adc2, DAC1NCS, DAC1SCK, DAC1SDI);
            };

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(100), handler);
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
            bool enableMotors,
            ushort adc1, 
            ushort adc2,

            // motor1 pins
            FPGA.OutputSignal<bool> Motor1Enabled,
            FPGA.OutputSignal<bool> Motor1Pin1,
            FPGA.OutputSignal<bool> Motor1Pin2,

            // motor2 pins
            FPGA.OutputSignal<bool> Motor2Enabled,
            FPGA.OutputSignal<bool> Motor2Pin1,
            FPGA.OutputSignal<bool> Motor2Pin2
            )
        {
            sbyte motor1 = 0, motor2 = 0;

            Action handler = () =>
            {
                if( enableMotors )
                {
                    Controllers.ADCTest.ToMotorValue(adc1, out motor1);
                    Controllers.ADCTest.ToMotorValue(adc2, out motor2);
                }
                else
                {
                    motor1 = 0;
                    motor2 = 0;
                }
            };

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(100), handler);

            Drivers.L298.SingleMotorDriver(motor1, Motor1Pin1, Motor1Pin2, Motor1Enabled);
            Drivers.L298.SingleMotorDriver(motor2, Motor2Pin1, Motor2Pin2, Motor2Enabled);
        }

        static void KeypadToLEDs(
            FPGA.OutputSignal<bool> LED1,
            FPGA.OutputSignal<bool> LED2,
            FPGA.OutputSignal<bool> LED3,
            FPGA.OutputSignal<bool> LED4,

            FPGA.OutputSignal<bool> K7,
            FPGA.OutputSignal<bool> K6,
            FPGA.OutputSignal<bool> K5,
            FPGA.OutputSignal<bool> K4,
            FPGA.InputSignal<bool> K3,
            FPGA.InputSignal<bool> K2,
            FPGA.InputSignal<bool> K1,
            FPGA.InputSignal<bool> K0,

            FPGA.OutputSignal<bool> Bank1,
            FPGA.OutputSignal<bool> Bank2,

            out byte code
            )
        {
            byte internalCode = 0;
            FPGA.Config.Link(internalCode, out code);

            Drivers.QuokkaBoard.OutputBank(Bank1);
            Drivers.QuokkaBoard.InputBank(Bank2);

            bool internalLED1 = false, internalLED2 = false, internalLED3 = false, internalLED4 = false;
            FPGA.Config.Link(internalLED1, LED1);
            FPGA.Config.Link(internalLED2, LED2);
            FPGA.Config.Link(internalLED3, LED3);
            FPGA.Config.Link(internalLED4, LED4);

            Action keypadHandler = () =>
            {
                Drivers.Keypad4x4.ReadCode(K7, K6, K5, K4, K3, K2, K1, K0, out internalCode);

                internalLED1 = (internalCode & 1) > 0;
                internalLED2 = (internalCode & 2) > 0;
                internalLED3 = (internalCode & 4) > 0;
                internalLED4 = (internalCode & 8) > 0;
            };

            FPGA.Config.OnTimer(TimeSpan.FromMilliseconds(100), keypadHandler);
        }

        static void ReportADC(
            ushort adc1, 
            ushort adc2,
            FPGA.OutputSignal<bool> TXD)
        {
            Action reportHandler = () =>
            {
                DTO.ADC data = new DTO.ADC();
                data.IN1 = adc1;
                data.IN2 = adc2;
                Drivers.JSON.SerializeToUART<DTO.ADC>(data, TXD);
            };

            FPGA.Config.OnTimer(TimeSpan.FromSeconds(1), reportHandler);
        }
    }
}