using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Tentakel.Extensions.Logging.Loggers;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging
{
    internal static class SourceResolver
    {
        private static readonly List<Type> typeIgnoreList = new();
        private static readonly List<string> namespaceIgnoreList = new();

        static SourceResolver()
        {
            typeIgnoreList.Add(Type.GetType("System.Threading.ThreadHelper", true));
            typeIgnoreList.Add(typeof(Thread));
            typeIgnoreList.Add(typeof(ExecutionContext));
            typeIgnoreList.Add(typeof(BackgroundWorker));
            typeIgnoreList.Add(typeof(Trace));
            typeIgnoreList.Add(typeof(TraceSource));

            typeIgnoreList.Add(typeof(LoggerExtensions));
            typeIgnoreList.Add(typeof(LoggerSinkProvider));
            typeIgnoreList.Add(typeof(Logger));

            namespaceIgnoreList.Add(typeof(Trace).Namespace);
        }

        public static bool TryFindStackTraceSource(Type logSourceType, StackTrace stackTrace, out string stackTraceSource)
        {
            if (stackTrace == null) throw new ArgumentNullException(nameof(stackTrace));
            stackTraceSource = null;

            if (stackTrace.FrameCount == 0) return false;

            var frames = stackTrace.GetFrames();
            var frameList = frames.ToList();
            frameList.Reverse();

            foreach (var frame in frameList)
            {
                var method = frame.GetMethod();
                if (method == null) continue;
                if (method.DeclaringType == null) continue;
                if (method.DeclaringType == logSourceType) break;

                if (string.IsNullOrEmpty(method.DeclaringType.Namespace)) continue;
                if (method.DeclaringType.Namespace.StartsWith("Microsoft.Extensions.Logging")) break;
                if (method.DeclaringType.Namespace.StartsWith("Microsoft")) continue;
                if (namespaceIgnoreList.Contains(method.DeclaringType.Namespace)) continue;
                //if (method.DeclaringType.Namespace.StartsWith("Tentakel.Extensions.Logging")) break;
                if (method.DeclaringType.Namespace == typeof(Logger).Namespace) break;
                if (typeIgnoreList.Contains(method.DeclaringType)) continue;
                
                stackTraceSource = $"{method.DeclaringType}.{method.Name}()";
            }

            //Tentakel.Tracing.Test.SourceResolverTest+<>c__DisplayClass4_0.<TestTraceAsync>b__0
            //Tentakel.Tracing.Test.SourceResolverTest+<>c.<TestTraceAsync>b__4_0
            if (stackTraceSource != null && stackTraceSource.Contains("+<>"))
            {
                //stackTraceSource = stackTraceSource.Substring(0, stackTraceSource.Length - 2);
                stackTraceSource = stackTraceSource[..^2];
            }

            return !string.IsNullOrEmpty(stackTraceSource);
        }
    }
}