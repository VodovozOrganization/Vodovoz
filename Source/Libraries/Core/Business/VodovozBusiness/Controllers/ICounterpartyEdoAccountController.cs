using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Client;

namespace VodovozBusiness.Controllers
{
	/// <summary>
	/// Контракт контроллера работы с ЭДО аккаунтами
	/// </summary>
	public interface ICounterpartyEdoAccountController
	{
		/// <summary>
		/// Получение основного ЭДО аккаунта клиента по Id организации
		/// </summary>
		/// <param name="client">Клиент</param>
		/// <param name="organizationId">Id организации</param>
		/// <returns>Основной ЭДО аккаунт клиента</returns>
		CounterpartyEdoAccount GetDefaultCounterpartyEdoAccountByOrganizationId(Counterparty client, int? organizationId);
		/// <summary>
		/// Добавление ЭДО аккаунтов по умолчанию для клиента
		/// </summary>
		/// <param name="client">Клиент</param>
		void AddDefaultEdoAccountsToCounterparty(Counterparty client);
	}
}
