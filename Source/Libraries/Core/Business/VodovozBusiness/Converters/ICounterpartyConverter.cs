using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;
using VodovozBusiness.Domain.Client;

namespace Vodovoz.Converters
{
	public interface ICounterpartyConverter
	{
		/// <summary>
		/// Конвертация клиента <see cref="Counterparty"/> в информацию о нем для ЭДО <see cref="CounterpartyInfoForEdo"/>
		/// </summary>
		/// <param name="counterparty">Конвертируемый контрагент</param>
		/// <param name="counterpartyEdoAccount">Информация об аккаунте ЭДО клиента</param>
		/// <returns>Информация о клиенте для ЭДО</returns>
		CounterpartyInfoForEdo ConvertCounterpartyToCounterpartyInfoForEdo(
			Counterparty counterparty, CounterpartyEdoAccount counterpartyEdoAccount);
	}
}
