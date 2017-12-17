using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FPGA
{
    public interface IAssignExpression
    {
        void Capture();
        void Set();
    }

    class AssignExpression<T> : IAssignExpression
    {
        Action<T> _target;
        Func<T> _source;
        T _value = default(T);

        public AssignExpression(
            Action<T> target,
            Func<T> source)
        {
            _target = target;
            _source = source;
        }

        public void Capture()
        {
            _value = _source();
        }

        public void Set()
        {
            _target(_value);
        }
    }


    class RegisterAssignExpression<T> : IAssignExpression
    {
        Register<T> _target;
        T _source = default(T);

        public RegisterAssignExpression(
            Register<T> target,
            T source)
        {
            _target = target;
            _source = source;
        }

        public void Capture()
        {
        }

        public void Set()
        {
            _target.Set(_source);
        }
    }


    class SignalAssignExpression<T> : IAssignExpression
    {
        Signal<T> _target;
        T _value = default(T);

        public SignalAssignExpression(
            Signal<T> target,
            T value)
        {
            _target = target;
            _value = value;
        }

        public void Capture()
        {
        }

        public void Set()
        {
            _target.Toggle(_value);
        }
    }

    public static class Expressions
    {
        public static IAssignExpression Assign<T>(
            Func<T> source,
            Action<T> target
            )
        {
            return new AssignExpression<T>(target, source);
        }

        public static IAssignExpression AssignRegister<T>(
            T source,
            Register<T> target
            )
        {
            return new RegisterAssignExpression<T>(target, source);
        }

        public static IAssignExpression AssignSignal<T>(
            T source,
            Signal<T> target
            )
        {
            return new SignalAssignExpression<T>(target, source);
        }
    }

    public static class Runtime
    {
        // simulation is not supported yet
        static ManualResetEvent _shutdown = new ManualResetEvent(true);

        public static void Shutdown()
        {
            _shutdown.Set();
        }

        public static uint StateOf(object sequence)
        {
            throw new NotImplementedException("Not supported");
        }

        public static uint ResetSequence(object sequence)
        {
            throw new NotImplementedException("Not supported");
        }

        public static void Assign(
            params IAssignExpression[] expressions
            )
        {
            foreach(var expr in expressions )
            {
                expr.Capture();
            }

            foreach (var expr in expressions)
            {
                expr.Set();
            }
        }

        public static void WaitForShutdown()
        {
            _shutdown.WaitOne();
        }

        public static T Rol<T>(byte ammount, T value) where T : struct
        {
            if( typeof(T) == typeof(ulong))
            {
                if( ammount > 64 )
                {
                    throw new Exception($"Max rol is 64 bits for type {typeof(T)}");
                }

                ulong v = (ulong)(object)value;
                var result = ((v << ammount) | (v >> (byte)(64 - ammount)));

                return (T)(object)result;
            }

            if (typeof(T) == typeof(byte))
            {
                if (ammount > 8)
                {
                    throw new Exception($"Max rol is 8 bits for type {typeof(T)}");
                }

                byte v = (byte)(object)value;
                var result = (byte)((v << ammount) | (v >> (byte)(8 - ammount)));

                return (T)(object)result;
            }

            throw new NotImplementedException($"simulation is not implemented yet for type {typeof(T)}");
        }

        public static void Delay(long delayNanoSeconds)
        {
            throw new NotImplementedException("simulation is not implemented yet");
        }

        public static void Delay(TimeSpan delay)
        {
            throw new NotImplementedException("simulation is not implemented yet");
        }

        public static void SetBidirAsInput<T>(FPGA.BidirSignal<T> signal)
        {
            throw new NotImplementedException("simulation is not implemented yet");
        }

        public static void SetBidirAsOutput<T>(FPGA.BidirSignal<T> signal)
        {
            throw new NotImplementedException("simulation is not implemented yet");
        }

        public static void SetBidirMode<T>(FPGA.BidirSignal<T> signal, bool isOutput)
        {
            throw new NotImplementedException("simulation is not implemented yet");
        }

        public static void WaitForAllConditions(params Signal<bool>[] sources)
        {
            var events = sources.Select(s =>
            {
                var e = new ManualResetEvent(false);

                Action<bool> handler = null;
                handler = (value) =>
                {
                    if (value)
                    {
                        e.Set();
                        s.Usubscribe(handler);
                    }
                };

                s.Subscribe(handler);

                return e;
            }).ToArray();

            WaitHandle.WaitAll(events);
        }

        class WaitForAllConditionsArgs
        {
            public Func<bool> Source;
            public ManualResetEvent Event = new ManualResetEvent(false);
        }

        /// <summary>
        /// limited function, poll for values of each source and completes when all are true
        /// this will not form if source is attached to signal, signal toggling will be lost
        /// </summary>
        /// <param name="sources"></param>
        public static void WaitForAllConditions(params Func<bool>[] sources)
        {
            var events = sources.Select(s =>
            {
                var args = new WaitForAllConditionsArgs() { Source = s };

                Task.Factory.StartNew((source) =>
                {
                    var a = source as WaitForAllConditionsArgs;
                    while (!a.Source())
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(50)).Wait();
                    }

                    a.Event.Set();

                }, args);

                return args.Event;
            }).ToArray();

            WaitHandle.WaitAll(events);
        }
    }
}
