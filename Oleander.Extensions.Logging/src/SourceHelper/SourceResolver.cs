using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Oleander.Extensions.Logging.BackgroundWork;
using Oleander.Extensions.Logging.Loggers;
using Oleander.Extensions.Logging.Providers;

namespace Oleander.Extensions.Logging.SourceHelper
{
    [SuppressMessage("ReSharper", "ReplaceSubstringWithRangeIndexer")]
    internal static class SourceResolver
    {
        private static readonly List<Type> typeIgnoreList = [];
        private static readonly List<string> namespaceIgnoreList = [];

        static SourceResolver()
        {

            var t = Type.GetType("System.Threading.ThreadHelper", false);
            if (t != null) typeIgnoreList.Add(t);

            typeIgnoreList.Add(typeof(Thread));
            typeIgnoreList.Add(typeof(ExecutionContext));
            typeIgnoreList.Add(typeof(BackgroundWorker));
            typeIgnoreList.Add(typeof(Trace));
            typeIgnoreList.Add(typeof(TraceSource));
            typeIgnoreList.Add(typeof(LoggerExtensions));
            typeIgnoreList.Add(typeof(LoggerSinkProvider));
            typeIgnoreList.Add(typeof(Logger));

            var traceNamespace = typeof(Trace).Namespace;
            if (traceNamespace != null) namespaceIgnoreList.Add(traceNamespace);
        }

        public static bool TryFindFromStackTrace(Type? logSourceType, StackTrace? stackTrace, [MaybeNullWhen(false)]out string stackTraceSource)
        {
            stackTraceSource = null;

            if (stackTrace == null) return false;
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
                //if (method.DeclaringType.Namespace.StartsWith("Oleander.Extensions.Logging")) break;
                if (method.DeclaringType.Namespace == typeof(Logger).Namespace) break;
                if (typeIgnoreList.Contains(method.DeclaringType)) continue;

                stackTraceSource = $"{method.DeclaringType}.{method.Name}";
            }

            //Oleander.Tracing.Test.SourceResolverTest+<>c__DisplayClass4_0.<TestTraceAsync>b__0
            //Oleander.Tracing.Test.SourceResolverTest+<>c.<TestTraceAsync>b__4_0
            if (stackTraceSource != null && stackTraceSource.Contains("+<>"))
            {
                //stackTraceSource = stackTraceSource[..^2];
                stackTraceSource = stackTraceSource.Substring(0, stackTraceSource.Length - 2);
            }

            return !string.IsNullOrEmpty(stackTraceSource);
        }

        public static bool TryFindFromAttributes(IDictionary<string, object> attributes, [MaybeNullWhen(false)] out string source)
        {
            source = null;

            if (attributes.TryGetValue("{CallingAssembly}", out var value) && value is string assemblyFullName &&
                attributes.TryGetValue("{CallerFilePath}", out value) && value is string callerFilePath &&
                attributes.TryGetValue("{CallerMemberName}", out value) && value is string callerMemberName &&
                attributes.TryGetValue("{CallerLineNumber}", out value) && value is int callerLineNumber)
            {
                var callerFilePathReplaced = callerFilePath.Replace('\\', '.').Replace('/', '.');
                var assemblyName = assemblyFullName.Split([", "], StringSplitOptions.RemoveEmptyEntries)[0];
                var indexOf = callerFilePathReplaced.ToLower().IndexOf(assemblyName.ToLower(), StringComparison.Ordinal);

                if (indexOf == -1)
                {
                    source = $"{callerFilePathReplaced}.{callerMemberName}[{callerLineNumber}]";
                    return true;
                }

                var typeName = callerFilePathReplaced.Substring(indexOf + assemblyName.Length + 1);

                // ReSharper disable once UseIndexFromEndExpression
                if (typeName[typeName.Length - 3] == '.')
                {
                    typeName = typeName.Substring(0, typeName.Length - 3);
                }

                source = $"{assemblyName}.{typeName}.{callerMemberName}[{callerLineNumber}]";
                return true;
            }

            return false;
        }
    }
}