using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace Edo.Transport
{
	// Принимает UoW параметром, а не берёт IPublishEndpoint из DI: вызывается и с десктопа, где у диалогов свои UoW.
	// Событие пишется в outbox транзакции этого UoW
	public interface IEdoRequestCreatedEventPublisher
	{
		/// <summary>
		/// Публикует событие в транзакции <paramref name="uow"/>, в которой сохраняется заявка: уйдёт после её коммита.
		/// Вызывайте до Commit
		/// </summary>
		Task Publish(
			IUnitOfWork uow,
			int requestId,
			string operation,
			CancellationToken cancellationToken = default);
	}
}
