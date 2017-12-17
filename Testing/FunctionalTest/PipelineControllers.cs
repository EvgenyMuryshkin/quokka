﻿using FPGA.Attributes;
using System;
using System.Threading.Tasks;

namespace FunctionalTest
{
    [BoardConfig(Name = "Quokka")]
    public static class Pipeline_RegisterOverride
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            Action handler = () =>
            {
                byte data = 0;
                Drivers.UART.Read(RXD, out data);

                Func<byte> dFunc = () => (byte)(data * 2);
                FPGA.Signal<bool> writeEnable = false;

                FPGA.Register<byte> result = new FPGA.Register<byte>(0);

                FPGA.Config.RegisterOverride(result, dFunc, writeEnable);

                FPGA.Runtime.Assign(FPGA.Expressions.AssignSignal(true, writeEnable));

                Drivers.UART.Write(result, TXD);
            };
            bool trigger = true;
            FPGA.Config.OnSignal(trigger, handler);
        }
    }

    [BoardConfig(Name = "Quokka")]
    public static class Pipeline_Baseline64BitAdder
    {
        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            Action handler = () =>
            {
                byte data = 0;
                ulong tmp = 0;
                ulong op1 = 0, op2 = 0;

                for(byte i = 0; i < 2; i++)
                {
                    for(byte j = 0; j < 8; j++)
                    {
                        Drivers.UART.Read(RXD, out data);
                        tmp = ((ulong)data << 56) | (tmp >> 8);
                    }

                    if ( i == 0 )
                    {
                        op1 = tmp;
                    }
                    else
                    {
                        op2 = tmp;
                    }
                }

                tmp = op1 + op2;

                for( byte j = 0; j < 8; j++)
                {
                    Drivers.UART.Write((byte)tmp, TXD);
                    tmp = tmp >> 8;
                }
            };
            bool trigger = true;
            FPGA.Config.OnSignal(trigger, handler);
        }
    }

    [BoardConfig(Name = "Quokka")]
    public static class Pipeline_Pipelined64BitAdder
    {
        public static void PipelinedAdder(ulong op1, ulong op2, out ulong result)
        {
            Func<uint> op1_lo = () => (uint)op1;
            Func<uint> op1_hi = () => (uint)(op1 >> 32);

            Func<uint> op2_lo = () => (uint)op2;
            Func<uint> op2_hi = () => (uint)(op2 >> 32);

            // TODO invocation in cast
            Func<uint> carry = () => (uint)((op1_lo() + op2_lo()) >> 32);

            uint res_lo = op1_lo() + op2_lo();
            uint res_hi = op1_hi() + op2_hi() + carry();

            result = (res_hi << 32);
            result = result | res_lo;
        }

        public static async Task Aggregator(
            FPGA.InputSignal<bool> RXD,
            FPGA.OutputSignal<bool> TXD
            )
        {
            Action handler = () =>
            {
                byte data = 0;
                ulong tmp = 0;
                ulong op1 = 0, op2 = 0;

                for (byte i = 0; i < 2; i++)
                {
                    for (byte j = 0; j < 8; j++)
                    {
                        Drivers.UART.Read(RXD, out data);
                        tmp = ((ulong)data << 56) | (tmp >> 8);
                    }

                    if (i == 0)
                    {
                        op1 = tmp;
                    }
                    else
                    {
                        op2 = tmp;
                    }
                }

                FunctionalTest.Pipeline_Pipelined64BitAdder.PipelinedAdder(op1, op2, out tmp);

                for (byte j = 0; j < 8; j++)
                {
                    Drivers.UART.Write((byte)tmp, TXD);
                    tmp = tmp >> 8;
                }
            };
            bool trigger = true;
            FPGA.Config.OnSignal(trigger, handler);
        }
    }
}
