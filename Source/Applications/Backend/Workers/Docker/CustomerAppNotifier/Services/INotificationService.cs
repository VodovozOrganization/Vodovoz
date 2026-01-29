using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts.Events;
using Vodovoz.Domain.Client;

namespace CustomerAppNotifier.Services
{
	/// <summary>
	/// Отправщик событий/уведомлений в ИПЗ
	/// </summary>
	public interface INotificationService
	{
		/// <summary>
		/// Уведомление о сопоставлении клиента с пользователем ИПЗ
		/// </summary>
		/// <param name="counterpartyDto">Данные уведомления</param>
		/// <param name="counterpartyFrom">Откуда клиент</param>
		/// <returns></returns>
		Task<int> NotifyOfCounterpartyAssignAsync(
			RegisteredNaturalCounterpartyDto counterpartyDto, CounterpartyFrom counterpartyFrom);
		/// <summary>
		/// Отправка события для разлогинивания пользователя в ИПЗ
		/// </summary>
		/// <param name="logoutEvent">Данные события</param>
		/// <param name="source">ИПЗ</param>
		/// <returns></returns>
		Task<bool> SendLogoutEventAsync(LogoutLegalAccountEvent logoutEvent, Source source);
	}
}
