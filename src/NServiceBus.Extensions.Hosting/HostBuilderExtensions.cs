namespace NServiceBus
{
    using System;
    using Extensions.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Extension methods to configure NServiceBus for the .NET Core generic host.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Configures the host to start an NServiceBus endpoint.
        /// </summary>
        public static IHostBuilder UseNServiceBus(
            this IHostBuilder hostBuilder,
            Func<HostBuilderContext, EndpointConfiguration> endpointConfigurationBuilder,
            Action<IServiceCollection, IConfiguration> configureServicesForHandlers = null)
        {
            hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                var newServiceCollection = new ServiceCollection();
                configureServicesForHandlers?.Invoke(newServiceCollection, ctx.Configuration);

                var endpointConfiguration = endpointConfigurationBuilder(ctx);
                var startableEndpoint = EndpointWithExternallyManagedContainer.Create(endpointConfiguration, new ServiceCollectionAdapter(newServiceCollection));

                newServiceCollection.AddSingleton(_ => startableEndpoint.MessageSession.Value);
                serviceCollection.AddTransient<IHostedService>(_ => new NServiceBusHostedService(startableEndpoint, newServiceCollection.BuildServiceProvider()));
            });

            return hostBuilder;
        }
    }
}