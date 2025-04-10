
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace GoHorse.Madiator;

internal sealed class Mediator(IServiceProvider provider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, Type> _requestHandlerCache = new();
    private static readonly ConcurrentDictionary<Type, List<Type>> _behaviorCache = new();
    private static readonly ConcurrentDictionary<Type, List<Type>> _notificationHandlerCache = new();

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = _requestHandlerCache
            .GetOrAdd(
                requestType,
                type => typeof(IRequestHandler<,>)
                    .MakeGenericType(type, typeof(TResponse))
            );

        dynamic handler = provider.GetRequiredService(handlerType);

        var behaviorTypes = _behaviorCache
            .GetOrAdd(
                requestType, 
                type => [.. provider
                    .GetServices(typeof(IPipelineBehavior<,>)
                    .MakeGenericType(type, typeof(TResponse)))
                    .Select(b => b.GetType())
                ]
            );

        var behaviors = behaviorTypes
            .Select(provider.GetRequiredService)
            .Cast<dynamic>()
            .Reverse()
            .ToList();

        RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.Handle((dynamic)request, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle((dynamic)request, cancellationToken, next);
        }

        return await handlerDelegate();
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var notificationType = notification.GetType();
        var handlerTypes = _notificationHandlerCache
            .GetOrAdd(
                notificationType, 
                type => [.. provider
                    .GetServices(typeof(INotificationHandler<>)
                    .MakeGenericType(type))
                    .Select(h => h.GetType())
                ]
            );

        foreach (var handlerType in handlerTypes)
        {
            dynamic handler = provider.GetRequiredService(handlerType);
            await handler.Handle((dynamic)notification, cancellationToken);
        }
    }
}
