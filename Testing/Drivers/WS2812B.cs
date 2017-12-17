using System;

namespace Drivers
{
    public static class WS2812B
    {
        public static void SyncWrite(
            uint[] rgb, 
            uint offset,
            int length,
            out bool DOUT)
        {
            DOUT = false;

            for(uint addr = 0; addr < length; addr++)
            {
                uint pixel = 0;
                uint actualAddr = addr + offset;
                pixel = rgb[actualAddr];

                uint ordered = (uint)(
                    ((byte)(pixel >> 8) << 16) | // green
                    ((byte)(pixel >> 16) << 8) | // red
                    (byte)(pixel)
                    );

                for(byte bit = 0; bit < 24; bit++)
                {
                    bool bitValue = FPGA.Config.Bit(ordered, 23);

                    if( bitValue )
                    {
                        DOUT = true;
                        //FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(900));
                        //FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(850));
                        FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(9000));
                        DOUT = false;
                        //FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(350));
                        //FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(150));
                        FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(3500));
                    }
                    else
                    {
                        DOUT = true;
                        //FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(350));
                        //FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(300));
                        FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(3500));
                        DOUT = false;
                        //FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(900));
                        //FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(700));
                        FPGA.Runtime.Delay(TimeSpanEx.FromNanoseconds(9000));
                    }

                    ordered = ordered << 1;
                }
            }

            DOUT = false;
            FPGA.Runtime.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}
