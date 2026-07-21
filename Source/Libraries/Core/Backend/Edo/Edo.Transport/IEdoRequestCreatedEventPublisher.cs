using System.Threading;
using System.Threading.Tasks;

namespace Edo.Transport
{
	public interface IEdoRequestCreatedEventPublisher
	{
		Task Publish(
			int requestId,
			string operation,
			CancellationToken cancellationToken = default);
	}
}
