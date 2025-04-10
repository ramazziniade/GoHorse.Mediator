using System.Threading;
using System.Threading.Tasks;

namespace GoHorse.Madiator;

public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}
