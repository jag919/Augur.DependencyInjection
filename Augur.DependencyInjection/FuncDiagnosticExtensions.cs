using System;
using System.Linq;
using System.Reflection;

namespace Augur.DependencyInjection
{

    /// <summary>
    /// These extensions are primarily meant to help make a more meaningful representation of Func<> delegates
    /// primarily for diagnostic purposes.
    /// </summary>
    /// <remarks>
    /// Expression<Func<>> would produce more useful strings generally.
    /// </remarks>
    public static class FuncDiagnosticExtensions
    {
        public static string ToDiagnosticString<T1>(this Func<T1> func)
        {
            return ToDiagnosticString(func.Target, func.Method);
        }

        public static string ToDiagnosticString<T1, T2>(this Func<T1, T2> func)
        {
            return ToDiagnosticString(func.Target, func.Method);
        }

        public static string ToDiagnosticString<T1, T2, T3>(this Func<T1, T2, T3> func)
        {
            return ToDiagnosticString(func.Target, func.Method);
        }

        public static string ToDiagnosticString<T1, T2, T3, T4>(this Func<T1, T2, T3, T4> func)
        {
            return ToDiagnosticString(func.Target, func.Method);
        }

        public static string ToDiagnosticString<T1, T2, T3, T4, T5>(this Func<T1, T2, T3, T4, T5> func)
        {
            return ToDiagnosticString(func.Target, func.Method);
        }

        public static string ToDiagnosticString(object target, MethodInfo method)
        {
            var targetString = (target?.GetType() ?? method.DeclaringType)?.ToDiagnosticString();
            var returnString = method.ReturnType.ToDiagnosticString();
            var parameterString = string.Join(",", method.GetParameters().Select(p => $"{p.ParameterType.ToDiagnosticString()} {p.Name}"));
            return $"{returnString} {targetString}.{method.Name}({parameterString})";
        }
    }
}
