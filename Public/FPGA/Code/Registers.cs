using System;
using System.Collections.Generic;

namespace FPGA
{
    internal class RegisterState<T>
    {
        internal T _value { get; private set; }
        HashSet<Action<T>> _subscribers = new HashSet<Action<T>>();

        public RegisterState()
        {
            _value = default(T);
        }

        public RegisterState(T value)
        {
            _value = value;
        }

        internal void Set(T value)
        {
            _value = value;

            var handlers = new HashSet<Action<T>>(_subscribers);
            foreach (var handler in handlers)
            {
                handler(value);
            }
        }
        public void Subscribe(Action<T> handler)
        {
            _subscribers.Add(handler);
        }

        public void Unsubscribe(Action<T> handler)
        {
            _subscribers.Remove(handler);
        }
    }

    public class Register<T>
    {
        RegisterState<T> _state;
        public Register()
        {
            _state = new RegisterState<T>();
        }

        public Register(T value)
        {
            _state = new RegisterState<T>(value);
        }

        public void Set(T value)
        {
            _state.Set(value);
        }

        internal Register(RegisterState<T> state)
        {
            _state = state;
        }

        internal void Subscribe(Action<T> handler)
        {
            _state.Subscribe(handler);
        }

        internal void Usubscribe(Action<T> handler)
        {
            _state.Unsubscribe(handler);
        }

        public static implicit operator T(Register<T> source)
        {
            return source._state._value;
        }

        public static implicit operator Register<T>(T source)
        {
            return new Register<T>(source);
        }

        public static implicit operator Func<T>(Register<T> source)
        {
            return () => { return source; };
        }
    }
}
