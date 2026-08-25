using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Создатель смс уведомления по доменному событию
	/// </summary>
	/// <typeparam name="TDomainEvent">Тип доменного события</typeparam>
	public interface ISmsNotificationCreator<in TDomainEvent>
	{
		/// <summary>
		/// Создаётся ли смс уведомление по указанному событию
		/// </summary>
		/// <param name="domainEvent">Доменное событие</param>
		bool CanCreate(TDomainEvent domainEvent);

		/// <summary>
		/// Создаёт смс уведомление в UnitOfWork вызывающего кода
		/// Уведомление попадёт в базу только при успешном коммите транзакции вызывающего
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="domainEvent">Доменное событие</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task CreateAsync(IUnitOfWork unitOfWork, TDomainEvent domainEvent, CancellationToken cancellationToken = default);
	}
}
