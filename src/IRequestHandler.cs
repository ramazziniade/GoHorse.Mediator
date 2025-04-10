using System.Threading;
using System.Threading.Tasks;

namespace GoHorse.Madiator;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
