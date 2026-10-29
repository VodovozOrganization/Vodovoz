using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transport
{
	// Принимает UoW параметром, а не берёт IPublishEndpoint из DI: вызывается и с десктопа, где у диалогов свои UoW.
	// Событие пишется в outbox транзакции этого UoW
	public interface IOrderEdoTaskCreatedEventPublisher
	{
		/// <summary>
		/// Кладёт в outbox событие запуска задачи ЭДО заказа в рамках переданного unit of work.
		/// Отправка в шину произойдёт после коммита транзакции: вызывайте до Commit.
		/// </summary>
		void Publish(IUnitOfWork uow, OrderEdoTask edoTask, CancellationToken cancellationToken = default);
	}
}
