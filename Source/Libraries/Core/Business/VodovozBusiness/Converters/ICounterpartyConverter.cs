using Vodovoz.Core.Data.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.Converters
{
	public interface ICounterpartyConverter
	{
		/// <summary>
		/// Конвертация клиента <see cref="Counterparty"/> в информацию о нем для ЭДО <see cref="CounterpartyInfoForEdo"/>
		/// </summary>
		/// <param name="counterparty">Конвертируемый контрагент</param>
		/// <returns>Информация о клиенте для ЭДО</returns>
		CounterpartyInfoForEdo ConvertCounterpartyToCounterpartyInfoForEdo(Counterparty counterparty);
	}
}
