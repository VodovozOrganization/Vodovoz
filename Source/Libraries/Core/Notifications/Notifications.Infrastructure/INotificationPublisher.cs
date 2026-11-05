using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Публикация уведомления по доменному событию. Уведомление сохраняется в транзакции переданного UoW
	/// (outbox MassTransit) и уходит после её коммита; без коммита не уходит.
	/// Сверх outbox учитывает настройки уведомления: отключено ли оно и можно ли отправить повтор
	/// (ключ дедупликации), а в одной транзакции оставляет только последнее уведомление по агрегату
	/// </summary>
	/// <remarks>
	/// UoW передаётся параметром: публикуют и с десктопа, где у диалогов свои UoW, и сервисы с UoW из фабрики.
	/// Внутри сейчас пишется в шину уведомлений (vhost notifications) через IUnitOfWorkOutbox; после перехода на один vhost —
	/// в основную шину, параметр UoW при этом остаётся
	/// </remarks>
	/// <typeparam name="TDomainEvent">Доменное событие уведомления</typeparam>
	public interface INotificationPublisher<in TDomainEvent>
	{
		/// <summary>
		/// Публикует уведомление в транзакцию <paramref name="unitOfWork"/>
		/// </summary>
		/// <returns>false — уведомление не опубликовано: отключено, повтор или для события нет сообщения</returns>
		Task<bool> TryPublishAsync(IUnitOfWork unitOfWork, TDomainEvent notificationEvent, CancellationToken cancellationToken = default);

		/// <summary>
		/// Синхронная версия <see cref="TryPublishAsync"/> (для десктопа)
		/// </summary>
		bool TryPublish(IUnitOfWork unitOfWork, TDomainEvent notificationEvent);
	}
}
