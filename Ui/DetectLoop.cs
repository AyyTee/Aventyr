using Game.Common;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method)]
    public class DetectLoop : OnMethodBoundaryAspect
    {
        readonly static List<StackMethod> _callStack = new List<StackMethod>();
        /// <summary>
        /// Number of TryExecutes deep we are. False means no recursive loop is detected yet.
        /// </summary>
        readonly static Stack<bool> _isThrown = new Stack<bool>();
        string _methodName;
        const int _indentSpaces = 4;
        readonly static bool _writeToConsole = false;
        static string Indent => new String(' ', _callStack.Count * _indentSpaces);

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(method, aspectInfo);
            _methodName = method.Name;
        }

        /// <summary>
        /// Invoke a function and return whether it got caught in a recursive loop. 
        /// IMPORTANT: Only call this with functions that have no side effects and whose parameters are the same for any nested calls. 
        /// Additionally, [DetectLoop] must be added to any methods that should be checked for recursive loops.
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

        public override void OnEntry(MethodExecutionArgs args)
        {
            base.OnEntry(args);

            // If TryExecute hasn't been called then we don't do anything here.
            if (!_isThrown.Any())
            {
                return;
            }

            // If a recursive loop is detected then we bubble up the result immediately.
            if (_isThrown.Peek())
            {
                args.FlowBehavior = FlowBehavior.Return;
                return;
            }

            if (_writeToConsole)
            {
                Console.WriteLine(Indent + $"Push {args.Instance} {_methodName}");
            }

            // Check the call stack to determine if we've seen this instance and method before.
            for (int i = _callStack.Count - 1; i >= 0; i--)
            {
                if (MethodsEqual(args, _callStack[i]))
                {
                    if (_writeToConsole)
                    {
                        Console.WriteLine(Indent + $"Loop with call depth {i}. Returns: {args.ReturnValue}");
                    }

                    _isThrown.Pop();
                    _isThrown.Push(true);
                    args.FlowBehavior = FlowBehavior.Return;
                    return;
                }
            }

            _callStack.Add(new StackMethod(args.Instance, _methodName));
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            // If TryExecute hasn't been called then we don't do anything here.
            if (!_isThrown.Any())
            {
                return;
            }

            DebugEx.Assert(MethodsEqual(args, _callStack.Last()), "Stack is corrupted.");
            _callStack.RemoveAt(_callStack.Count - 1);
            if (_writeToConsole)
            {
                Console.WriteLine(Indent + $"Pop  {args.Instance} {_methodName}  Returns: {args.ReturnValue}");
            }

            base.OnExit(args);
        }

        bool MethodsEqual(MethodExecutionArgs methodArgs, StackMethod stackMethod)
        {
            return ReferenceEquals(stackMethod.Instance, methodArgs.Instance) &&
                stackMethod.MethodName == _methodName;
        }

        struct StackMethod
        {
            public object Instance { get; }
            public string MethodName { get; }

            public StackMethod(object instance, string methodName)
            {
                Instance = instance;
                MethodName = methodName;
            }
        }
    }
}
