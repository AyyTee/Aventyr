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
        bool _writeToConsole { get; } = false;
        string Indent => new String(' ', _callStack.Count * _indentSpaces);

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(method, aspectInfo);
            _methodName = method.Name;
        }

        /// <summary>
        /// Invoke a function and return whether it got caught in a recursive loop. 
        /// IMPORTANT: Only call this with functions that have no side effects and pass constant parameters. 
        /// Additionally, [DetectLoop] must be added to any methods that can be called from this function, otherwise recursive loops might not be detected.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="result">Return value from invoked function.</param>
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

            if (_isThrown.Any() && _isThrown.Peek())
            {
                args.FlowBehavior = FlowBehavior.Return;
                return;
            }

            if (_writeToConsole)
            {
                Console.WriteLine(Indent + $"Push {args.Instance} { args.Method.Name}");
            }

            for (int i = _callStack.Count - 1; i >= 0; i--)
            {
                if (MethodsEqual(args, _callStack[i]))
                {
                    if (_writeToConsole)
                    {
                        Console.WriteLine(Indent + $"Loop at call depth {i}.  returns: {args.ReturnValue}");
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
                Console.WriteLine(Indent + $"Pop  {args.Instance} {args.Method.Name}  returns: {args.ReturnValue}");
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
