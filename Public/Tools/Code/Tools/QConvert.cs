using System;

namespace Quokka.Public.Tools
{
    public static class QConvert
    {
        public static int ToInt32(string input)
        {
            try
            {
                return Convert.ToInt32(input);
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to Int32", ex);
            }
        }

        public static int ToInt32(double input)
        {
            try
            {
                return Convert.ToInt32(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to Int32", ex);
            }
        }

        public static object ToInt64(double input)
        {
            try
            {
                return Convert.ToInt64(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to Int64", ex);
            }
        }

        public static long ToInt64(string input)
        {
            try
            {
                return Convert.ToInt64(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to Int64", ex);
            }
        }

        public static object ChangeType(object propValue, Type propertyType)
        {
            try
            {
                return Convert.ChangeType(propValue, propertyType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change type of '{propValue}' to {propertyType}", ex);
            }
        }

        public static double ToDouble(string input)
        {
            try
            {
                return Convert.ToDouble(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to double", ex);
            }
        }

        public static ushort ToUInt16(string input)
        {
            try
            {
                return Convert.ToUInt16(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to UInt16", ex);
            }
        }

        public static uint ToUInt32(double input)
        {
            try
            {
                return Convert.ToUInt32(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to UInt32", ex);
            }
        }

        public static uint ToUInt32(string input)
        {
            try
            {
                return Convert.ToUInt32(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to UInt32", ex);
            }
        }

        public static ulong ToUInt64(string input)
        {
            try
            {
                return Convert.ToUInt64(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to UInt64", ex);
            }
        }

        public static ulong ToUInt64(string input, int convertionBase)
        {
            try
            {
                return Convert.ToUInt64(input, convertionBase);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to UInt64", ex);
            }
        }

        public static int ToInt32(string input, int convertionBase)
        {
            try
            {
                return Convert.ToInt32(input, convertionBase);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to UInt64", ex);
            }
        }

        public static ulong ToUInt64(double input)
        {
            try
            {
                return Convert.ToUInt64(input);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to contert '{input}' to UInt64", ex);
            }
        }
    }
}
