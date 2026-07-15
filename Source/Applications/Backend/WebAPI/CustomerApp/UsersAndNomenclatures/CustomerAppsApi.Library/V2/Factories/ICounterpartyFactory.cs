using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Factories
{
	/// <summary>
	/// Фабрика по клиенту
	/// </summary>
	public interface ICounterpartyFactory
	{
		/// <summary>
		/// Создание клиента из ИПЗ
		/// </summary>
		/// <param name="counterpartyDto">Данные по клиенту</param>
		/// <returns></returns>
		Counterparty CreateCounterpartyFromExternalSource(CounterpartyDto counterpartyDto);
		/// <summary>
		/// Создание данных по долгу клиента по бутылям
		/// </summary>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <param name="debt">Долг</param>
		/// <returns></returns>
		CounterpartyBottlesDebtDto CounterpartyBottlesDebtDto(int counterpartyId, int debt);
	}
}
