using System;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Domain.Client;

namespace Vodovoz.Converters
{
	public interface ICounterpartyContractConverter
	{
		/// <summary>
		/// Конвертация клиентского контракта(договора) <see cref="CounterpartyContract"/>
		/// в информацию о нем для ЭДО <see cref="CounterpartyContractInfoForEdo"/>
		/// </summary>
		/// <param name="contract">Контракт, который будет конвертирован</param>
		/// <param name="dateTime">Дата, для получения юридического адреса организации</param>
		/// <returns>Информация о контракте для ЭДО</returns>
		CounterpartyContractInfoForEdo ConvertCounterpartyContractToCounterpartyContractInfoForEdo(
			CounterpartyContract contract, DateTime dateTime);
	}
}
