using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace Edo.Docflow.Converters
{
	public interface ICounterpartyConverter
	{
		/// <summary>
		/// Конвертация клиента <see cref="CounterpartyEntity"/> в информацию о нем для ЭДО <see cref="CounterpartyInfoForEdo"/>
		/// </summary>
		/// <param name="counterparty">Конвертируемый контрагент</param>
		/// <param name="counterpartyEdoAccount">Информация об аккаунте ЭДО клиента</param>
		/// <returns>Информация о клиенте для ЭДО</returns>
		CounterpartyInfoForEdo ConvertCounterpartyToCounterpartyInfoForEdo(
			CounterpartyEntity counterparty, CounterpartyEdoAccountEntity counterpartyEdoAccount);
	}
}
