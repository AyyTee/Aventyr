//#define AFFINE_ENABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace EditorLogic
{
    [AttributeUsage(
        AttributeTargets.Class | 
        AttributeTargets.Field | 
        AttributeTargets.Event | 
        AttributeTargets.Property | 
        AttributeTargets.Assembly), 
        Serializable, Conditional("AFFINE_ENABLED")]
    public class AffineMemberAttribute : LocationInterceptionAspect
    {
        public void ThreadCheck(LocationInterceptionArgs args)
        {
            //Threads are safe if they are synchonized with Dispatcher.Invoke.  This is very slow and is prone to false positives.
            {
                StackTrace stackTrace = new StackTrace();
                for (int i = 1; i < stackTrace.FrameCount; i++)
                {
                    if (stackTrace.GetFrame(i).GetMethod().Name == "Invoke")
                    {
                        return;
                    }
                }
            }

            //Ignore thread safety if the attribute AffineIgnore is present.
            {
                var t = args.Instance.GetType();

                Type type = args.Location.GetType();

                MemberInfo[] info = t.GetMember(args.LocationName);
                if (info.Length > 0 && IsDefined(info[0], typeof(AffineIgnore)))
                {
                    return;
                }
            }

            Thread thread;
            Debug.Assert(AffineAttribute.ThreadMap.TryGetValue(args.Instance, out thread));
            Debug.Assert(
                thread == Thread.CurrentThread, 
                "Instance cannot be modified from any thread other than instantiating thread."
                );
        }

        public override void OnSetValue(LocationInterceptionArgs args)
        {
            ThreadCheck(args);
            base.OnSetValue(args);
        }

        public override void OnGetValue(LocationInterceptionArgs args)
        {
            ThreadCheck(args);
            base.OnGetValue(args);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly), Serializable, Conditional("AFFINE_ENABLED")]
    public class AffineAttribute : InstanceLevelAspect
    {
        public static ConditionalWeakTable<object, Thread> ThreadMap = new ConditionalWeakTable<object, Thread>();

        public override void RuntimeInitializeInstance()
        {
            Thread thread;
            //An instance will be initilized multiple times if it is Affine and inherits from an Affine class.
            //Make sure in such cases, the instance isn't added multiple times to the ThreadMap.
            if (!ThreadMap.TryGetValue(Instance, out thread))
            {
                ThreadMap.Add(Instance, Thread.CurrentThread);
            }
            base.RuntimeInitializeInstance();
        }

        public static void SetOwner(object instance, Thread newOwner)
        {
            Thread thread;
            Debug.Assert(ThreadMap.TryGetValue(instance, out thread), "Instance is not associated with a thread.");
            ThreadMap.Remove(instance);
            ThreadMap.Add(instance, newOwner);
        }
    }

    [AttributeUsage(
        AttributeTargets.Event | 
        AttributeTargets.Field | 
        AttributeTargets.Property), 
        Conditional("AFFINE_ENABLED")]
    public class AffineIgnore : Attribute
    {
    }
}