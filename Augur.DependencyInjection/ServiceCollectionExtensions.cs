using Augur.DependencyInjection;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Adds additional ServiceCollection registration methods
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adda a service with a singleton lifeecycle to the <see cref="IServiceCollection"/> with the specified ServiceBuilder extensions
        /// </summary>
        /// <typeparam name="T">The concrete type being registered</typeparam>
        /// <param name="services">The service collection to add to</param>
        /// <param name="options">the additional registration options</param>
        /// <returns>A fluent interface</returns>
        public static IServiceCollection AddSingletonWithOptions<T>(this IServiceCollection services, Action<ServiceConstructionBuilder<T>> options)
        {
            return AddWithOptions<T>(services, ServiceLifetime.Singleton, options);
        }

        /// <summary>
        /// Adda a service with a scoped lifeecycle to the <see cref="IServiceCollection"/> with the specified ServiceBuilder extensions
        /// </summary>
        /// <typeparam name="T">The concrete type being registered</typeparam>
        /// <param name="services">The service collection to add to</param>
        /// <param name="options">the additional registration options</param>
        /// <returns>A fluent interface</returns>
        public static IServiceCollection AddScopedWithOptions<T>(this IServiceCollection services, Action<ServiceConstructionBuilder<T>> options)
        {
            return AddWithOptions<T>(services, ServiceLifetime.Scoped, options);
        }

        /// <summary>
        /// Adda a service with a transient lifeecycle to the <see cref="IServiceCollection"/> with the specified ServiceBuilder extensions
        /// </summary>
        /// <typeparam name="T">The concrete type being registered</typeparam>
        /// <param name="services">The service collection to add to</param>
        /// <param name="options">the additional registration options</param>
        /// <returns>A fluent interface</returns>
        public static IServiceCollection AddTransientWithOptions<T>(this IServiceCollection services, Action<ServiceConstructionBuilder<T>> options)
        {
            return AddWithOptions<T>(services, ServiceLifetime.Transient, options);
        }

        /// <summary>
        /// Adda a service with the specified lifeecycle to the <see cref="IServiceCollection"/> with the specified ServiceBuilder extensions
        /// </summary>
        /// <typeparam name="T">The concrete type being registered</typeparam>
        /// <param name="services">The service collection to add to</param>
        /// <param name="lifetime">The lifetime for instances of T</param>
        /// <param name="options">the additional registration options</param>
        /// <returns>A fluent interface</returns>
        public static IServiceCollection AddWithOptions<T>(this IServiceCollection services, ServiceLifetime lifetime, Action<ServiceConstructionBuilder<T>> options)
        {
            var builder = new ServiceConstructionBuilder<T>();
            options(builder);
            builder.BuildServiceDescriptors(lifetime).ToList().ForEach(services.Add);
            return services;
        }

        /// <summary>
        /// Adda a service with the specified lifeecycle to the <see cref="IServiceCollection"/> with the specified ServiceBuilder extensions
        /// </summary>
        /// <param name="services">The service collection to add to</param>
        /// <param name="implementationType">The concrete type being registered</param>
        /// <param name="lifetime">The lifetime for instances of T</param>
        /// <param name="options">the additional registration options</param>
        /// <returns>A fluent interface</returns>
        public static IServiceCollection AddWithOptions(this IServiceCollection services, Type implementationType, ServiceLifetime lifetime, Action<IRuntimeServiceConstructionBuilder> options)
        {
            var builderType = typeof(ServiceConstructionBuilder<>).MakeGenericType(implementationType);
            var builder = Activator.CreateInstance(builderType);
            options((IRuntimeServiceConstructionBuilder)builder);
            ((ServiceConstructionBuilder)builder).BuildServiceDescriptors(lifetime).ToList().ForEach(services.Add);
            return services;
        }

        /// <summary>
        /// Replaces existing service registrations with a decorator pattern
        /// </summary>
        /// <remarks>
        /// This will replace registrations up to this point but not future ones.
        /// </remarks>
        /// <typeparam name="TService">The type of the service to decorate</typeparam>
        /// <typeparam name="TDecorator">The decorator implementation</typeparam>
        /// <param name="services">The service collection to add to</param>
        /// <returns>A fluent interface</returns>
        public static IServiceCollection ReplaceWithDecorator<TService, TDecorator>(this IServiceCollection services)
          where TDecorator : TService
        {
            // This will replace registrations up to this point but not future ones.  If this is not good enough, create a
            // service collection wrapper, that will hold these decorator patterns, then on the call to BuildServiceProvider
            // do all the replacements before making the wrapped call.
            var decorate = services.Where(sd => sd.ServiceType == typeof(TService)).ToList();
            decorate.ForEach(sd => services.Remove(sd));

            foreach (var descriptor in decorate)
            {
                ConstructorMap constructorMap;

                if (descriptor.ImplementationInstance != null)
                {
                    constructorMap = new ValueConstructorMap<TService>((TService)descriptor.ImplementationInstance);
                }
                else if (descriptor.ImplementationType != null)
                {
                    var factoryType = typeof(ServiceConstructionFactory<>).MakeGenericType(descriptor.ImplementationType);
                    var factory = Activator.CreateInstance(factoryType) as IServiceFactory;
                    constructorMap = new DelegateConstructorMap<TService>(factory.Create);
                }
                else
                {
                    constructorMap = new DelegateConstructorMap<TService>(descriptor.ImplementationFactory);
                }

                services.Add(new ServiceDescriptor(typeof(TService), new ServiceConstructionFactory<TDecorator>(new[] { constructorMap }, null).Create, descriptor.Lifetime));
            }

            return services;
        }

        /// <summary>
        /// Used to examine all of the registrations made to the <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">The service collection to add to</param>
        /// <param name="writeLine">action for writing a line, defaults to Trace.WriteLine</param>
        /// <returns>A fluent interface</returns>
        public static IServiceCollection WriteDiagnostics(this IServiceCollection services, Action<string> writeLine = null)
        {
            var writeTo = writeLine ?? TraceLine;

            writeTo("==== SERVICECOLLECTION DESCRIPTORS ====");

            foreach (var descriptor in services)
            {
                string implementation;

                if (descriptor.ImplementationType != null)
                {
                    implementation = $"Type, Of: {descriptor.ImplementationType.ToDiagnosticString()}";
                }
                else if (descriptor.ImplementationInstance != null)
                {
                    implementation = $"Instance, Of: {descriptor.ImplementationInstance.GetType().ToDiagnosticString()}";
                }
                else
                {
                    implementation = $"Expression, Of: {descriptor.ImplementationFactory.ToDiagnosticString()}";
                }

                writeTo($"ServiceType: {descriptor.ServiceType.ToDiagnosticString()}, Lifetime: {descriptor.Lifetime}, Implementation: {implementation}");
            }

            writeTo("==== DONE ====");

            return services;
        }

        private static void TraceLine(string line)
        {
            System.Diagnostics.Trace.WriteLine(line);
        }
    }
}
