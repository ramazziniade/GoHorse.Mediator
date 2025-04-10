using Microsoft.Extensions.DependencyInjection;

namespace GoHorse.Madiator
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRequestHandlers(this IServiceCollection services, System.Reflection.Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                foreach (var iface in interfaces)
                {
                    if (iface.IsGenericType)
                    {
                        var def = iface.GetGenericTypeDefinition();
                        if (def == typeof(IRequestHandler<,>) || def == typeof(INotificationHandler<>))
                        {
                            services.AddTransient(iface, type);
                        }

                        if (def == typeof(IPipelineBehavior<,>))
                        {
                            services.AddTransient(iface, type);
                        }
                    }
                }
            }

            return services;
        }
    }
}
