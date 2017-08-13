//#define WriteStackToConsole
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
    public class DetectLoopAspect : OnMethodBoundaryAspect
    {
        readonly static List<StackMethod> _callStack = new List<StackMethod>();
        string _methodName;
        const int _indentSpaces = 4;


        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            base.CompileTimeInitialize(method, aspectInfo);
            _methodName = method.Name;
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
#if WriteStackToConsole
            Console.WriteLine(new String(' ', _callStack.Count * _indentSpaces) + "Push " + args.Instance + " " + args.Method.Name);
#endif
            base.OnEntry(args);
            for (int i = _callStack.Count - 1; i >= 0; i--)
            {
                if (MethodsEqual(args, _callStack[i]))
                {
#if WriteStackToConsole
                    Console.WriteLine(new String(' ', _callStack.Count * _indentSpaces) + $"Loop at call depth {i}.  returns: {args.ReturnValue}");
#endif
                    args.FlowBehavior = FlowBehavior.Return;
                    return;
                }
            }
            _callStack.Add(new StackMethod(args.Instance, _methodName));
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            DebugEx.Assert(MethodsEqual(args, _callStack.Last()), "Stack is corrupted.");
            _callStack.RemoveAt(_callStack.Count - 1);
            base.OnExit(args);
#if WriteStackToConsole
            Console.WriteLine(new String(' ', _callStack.Count * _indentSpaces) + $"Pop  {args.Instance} {args.Method.Name}  returns: {args.ReturnValue}");
#endif
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
