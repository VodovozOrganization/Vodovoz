using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transport
{
	public interface IOrderEdoTaskCreatedEventPublisher
	{
		/// <summary>
		/// Кладёт в outbox событие запуска задачи ЭДО заказа в рамках переданного unit of work.
		/// Фактическая отправка в шину произойдёт отдельно, силами OutboxWorker, после коммита транзакции.
		/// </summary>
		void Publish(IUnitOfWork uow, OrderEdoTask edoTask, CancellationToken cancellationToken = default);
	}
}
