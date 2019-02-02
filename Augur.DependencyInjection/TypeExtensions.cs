using System;
using System.Collections.Generic;
using System.Linq;

namespace Augur.DependencyInjection
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            return type.BaseType == typeof(object)
                ? Enumerable.Empty<Type>()
                : Enumerable
                    .Repeat(type.BaseType, 1)
                    .Concat(type.BaseType.GetBaseTypes());
        }

        public static IEnumerable<Type> GetTypeAndBaseTypes(this Type type)
        {
            return Enumerable.Repeat(type, 1).Concat(type.GetBaseTypes());
        }

        public static IEnumerable<Type> GetBaseTypesAndInterfaces(this Type type)
        {
            // GetInterfaces already returns all interfaces for base types, and interfaces of interfaces
            return GetBaseTypes(type).Concat(type.GetInterfaces()).Distinct();
        }

        public static bool IsNonSystemType(this Type type)
        {
            return !type.AssemblyQualifiedName.StartsWith("System", StringComparison.Ordinal);
        }

        public static void AssertIsAssignableFrom(this Type to, Type from)
        {
            if (!to.IsAssignableFrom(from))
            {
                throw new ArgumentException($"{to.Name} must be assignable from {from.Name}");
            }
        }

        public static void AssertIsSubclassOf(this Type subClassType, Type baseType)
        {
            if (!subClassType.IsSubclassOf(baseType))
            {
                throw new ArgumentException($"{subClassType.Name} must be a sub class of {baseType.Name}");
            }
        }

        public static bool IsNullableType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsGenericTypeOf(this Type type, Type genericType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
        }

        public static string ToDiagnosticString(this Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Namespace == "System" ? type.Name : type.FullName;
            }

            var generics = type.GenericTypeArguments.Select(gType => gType.ToDiagnosticString()).ToList();
            var trailing = $"`{generics.Count}";
            var name = type.Name.Replace(trailing, string.Empty, StringComparison.Ordinal);
            var ns = type.Namespace == "System" ? string.Empty : type.Namespace + ".";
            return $"{ns}{name}<{string.Join(",", generics)}>";
        }
    }
}
