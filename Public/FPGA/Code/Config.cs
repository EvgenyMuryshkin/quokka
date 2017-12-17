using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FPGA
{
    public enum CompareType
    {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
    }

    public enum MathType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Remainder,
    }

    public static class Config
    {
        public static TPackage Entity<TPackage>()
        {
            throw new Exception("simulation not implemented");
        }

        public static bool HighBit<T>(T data) where T : struct
        {
            throw new Exception("simulation not implemented");
        }

        public static bool Bit<T>(T data, ushort bit) where T : struct
        {
            throw new Exception("simulation not implemented");
        }

        public static TOp1 Math<TOp1, TOp2>(MathType type, TOp1 op1, TOp2 op2)
        {
            throw new Exception("simulation not implemented");
        }

        public static bool Compare<TOp1, TOp2>(TOp1 op1, CompareType type, TOp2 op2)
        {
            throw new Exception("simulation not implemented");
        }

        public static TOp1 LShift<TOp1, TOp2>(TOp1 op1, int by, TOp2 op2) 
            where TOp1 : struct
            where TOp2 : struct
        {
            throw new Exception("simulation not implemented");
        }

        public static TOp1 LShift<TOp1>(TOp1 op1, int by)
            where TOp1 : struct
        {
            throw new Exception("simulation not implemented");
        }

        public static byte SizeOf<T>(T value) where T : struct
        {
            throw new Exception("simulation not implemented");
        }

        public static void Default<T>(out T target, int value)
        {
            throw new Exception("simulation not implemented");
        }

        public static void Default<T>(out T target, T value)
        {
            throw new Exception("simulation not implemented");
        }

        public static void Suppress(string warning, params object[] items)
        {
        }

        public static void Default<T>(out FPGA.OutputSignal<T> target, T value)
        {
            throw new Exception("simulation not implemented");
        }

        public static void Link<T>(T source, out T target)
        {
            throw new Exception("simulation not implemented");
        }

        public static void Link<T>(Func<T> source, out T target)
        {
            throw new Exception("simulation not implemented");
        }

        public static void Link<T>(T source, FPGA.OutputSignal<T> target)
        {
        }

        public static void Link<T>(T source, FPGA.Signal<T> target)
        {
        }

        public static void Link<T>(FPGA.Signal<T> source, FPGA.OutputSignal<T> target)
        {
        }

        public static void Link<T>(Func<T> source, FPGA.OutputSignal<T> target)
        {
        }

        public static void Link<T>(Func<T> source, FPGA.Signal<T> target)
        {
        }

        public static void OnTimer(TimeSpan period, Action handler)
        {
            throw new NotImplementedException("simulation is not implemented yet");
        }

        public static void OnTimer(long period, Action handler)
        {
            throw new NotImplementedException("simulation is not implemented yet");
        }

        public static void OnSignal(object source, Action handler, uint instances = 1)
        {
            if( source is Signal<bool> signal)
            {
                signal.Subscribe((v) =>
                {
                    if (!v)
                        return;

                    var barrier = new ManualResetEvent(false);

                    foreach(var index in Enumerable.Range(0, (int)instances))
                    {
                        Task.Factory.StartNew(() => {
                            barrier.WaitOne();
                            handler();
                        });
                    }

                    // hack, race condition with set and wait for completion, task is finished before waiting is started
                    // delay task execution
                    Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
                    barrier.Set();
                });

                return;
            }

            throw new NotImplementedException("simulation is not implemented yet");
        }

        public static void OnRegisterWritten<T>(T data, Action handler, uint instances = 1)
        {
            throw new NotImplementedException("simulation is not implemented yet for native types, use Register<T> instead");
        }

        public static void OnRegisterWritten<T>(Register<T> register, Action handler, uint instances = 1)
        {
            register.Subscribe((v) =>
            {
                var barrier = new ManualResetEvent(false);

                foreach (var index in Enumerable.Range(0, (int)instances))
                {
                    Task.Factory.StartNew(() => {
                        barrier.WaitOne();
                        handler();
                    });
                }

                // hack, race condition with set and wait for completion, task is finished before waiting is started
                // delay task execution
                Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
                barrier.Set();
            });
        }

        public static void RegisterOverride<T>(Register<T> target, Func<T> source, Func<bool> writeEnable)
        {
            throw new Exception("Simulation not implemented yet");
        }

        public static void RegisterOverride<T>(Register<T> target, Signal<T> source, Signal<bool> writeEnable)
        {
            throw new Exception("Simulation not implemented yet");
        }

        public static void GenBreak()
        {
            // do nothing here, this is diagnostics method
        }

        public static void JSONDeserializer<T>(
            T data,
            byte inByte,
            FPGA.Signal<bool> deserialized) where T : new()
        {
            throw new Exception("Simulation not implemented yet");
        }

        public static void JSONSerializer<T>(
            T data,
            out FPGA.Signal<bool> hasMoreData,
            out FPGA.Signal<bool> triggerDequeue,
            out FPGA.Signal<bool> dataDequeued,
            out FPGA.Signal<byte> currentByte)
        {
            throw new Exception("Simulation not implemented yet");
        }
    }
}
