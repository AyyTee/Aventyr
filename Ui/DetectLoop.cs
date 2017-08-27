using Game.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui
{
    public static class DetectLoop
    {
        readonly static List<StackEntry> _callStack = new List<StackEntry>();
        /// <summary>
        /// Number of TryExecutes deep we are. False means no recursive loop is detected yet.
        /// </summary>
        readonly static Stack<bool> _isThrown = new Stack<bool>();
        const int _indentSpaces = 4;
        readonly static bool _writeToConsole = true;
        static string Indent => new String(' ', _callStack.Count * _indentSpaces);

        static string _output = "";

        /// <summary>
        /// Invoke a function and return whether it got caught in a recursive loop. 
        /// IMPORTANT: Only call this with functions that have no side effects and whose parameters are the same for any nested calls.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="func">Function to try executing.</param>
        /// <param name="result">Returned value from invoked function.</param>
        /// <returns>True if the method completed successfully, false if a recursive loop was detected.</returns>
        public static bool TryExecute<T>(Func<T> func, out T result)
        {
            _isThrown.Push(false);
            result = func.Invoke();
            return !_isThrown.Pop();
        }

        public static bool OnEntry(StackEntry current)
        {
            // If TryExecute hasn't been called then we don't do anything here.
            if (!_isThrown.Any() && !Debugger.IsAttached)
            {
                return true;
            }

            // If a recursive loop is detected then we bubble up the result immediately.
            if (_isThrown.Any() && _isThrown.Peek())
            {
                return false;
            }

            if (_writeToConsole)
            {
                Console.WriteLine(Indent + $"Push {current.Element} {current.ElementProperty}");
                _output += Indent + $"Push {current.Element} {current.ElementProperty}\n";
            }

            // Check the call stack to determine if we've seen this instance and method before.
            if (_callStack.Contains(current))
            {
                _callStack.Add(current);

                if (_writeToConsole)
                {
                    Console.WriteLine(Indent + $"Loop detected!");
                    _output += $"Loop detected!\n";
                }

                if (!_isThrown.Any() && Debugger.IsAttached)
                {
                    throw new StackOverflowException("Detected stack overflow early.");
                }

                _isThrown.Pop();
                _isThrown.Push(true);
                return false;
            }

            _callStack.Add(current);
            return true;
        }

        public static void OnExit(StackEntry current)
        {
            // If TryExecute hasn't been called then we don't do anything here.
            if (!_isThrown.Any() && !Debugger.IsAttached)
            {
                return;
            }

            DebugEx.Assert(current.Equals(_callStack.Last()), "Stack is corrupted.");
            _callStack.RemoveAt(_callStack.Count - 1);
            if (_writeToConsole)
            {
                Console.WriteLine(Indent + $"Pop  {current.Element} {current.ElementProperty}");
                _output += Indent + $"Pop  {current.Element} {current.ElementProperty}\n";
            }
        }

        public struct StackEntry : IDisposable
        {
            public Element Element { get; }
            public string ElementProperty { get; }

            public StackEntry(Element element, string elementProperty)
            {
                Element = element;
                ElementProperty = elementProperty;
            }

            public override string ToString()
            {
                return $"{nameof(StackEntry)}: {Element}.{ElementProperty}";
            }

            public void Dispose()
            {
                OnExit(this);
            }
        }
    }
}
