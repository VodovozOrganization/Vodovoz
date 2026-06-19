using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Интерфейс для публикации уведомлений в рамках транзакционного аутбокса. Позволяет создавать и сохранять сообщения в аутбоксе,
	/// которые будут опубликованы после успешного коммита транзакции.
	/// </summary>
	/// <typeparam name="TDomainEvent"></typeparam>
	public interface IOutboxNotificationPublisher<in TDomainEvent>
	{
		/// <summary>
		/// Ассинхронная версия публикации уведомления в рамках транзакционного аутбокса. Сообщение будет сохранено в аутбоксе и опубликовано после успешного коммита транзакции.
		/// </summary>
		Task<bool> TryPublishAsync(IUnitOfWork unitOfWork, TDomainEvent notificationEvent, CancellationToken cancellationToken = default);

		/// <summary>
		/// Публикация уведомления в рамках транзакционного аутбокса. Сообщение будет сохранено в аутбоксе и опубликовано после успешного коммита транзакции.
		/// </summary>
		bool TryPublish(IUnitOfWork unitOfWork, TDomainEvent notificationEvent);
	}
}
