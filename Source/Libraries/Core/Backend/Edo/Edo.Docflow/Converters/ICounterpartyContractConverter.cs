using System;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace Edo.Docflow.Converters
{
	public interface ICounterpartyContractConverter
	{
		/// <summary>
		/// Конвертация клиентского контракта(договора) <see cref="CounterpartyContractEntity"/>
		/// в информацию о нем для ЭДО <see cref="CounterpartyContractInfoForEdo"/>
		/// </summary>
		/// <param name="contract">Контракт, который будет конвертирован</param>
		/// <param name="dateTime">Дата, для получения юридического адреса организации</param>
		/// <returns>Информация о контракте для ЭДО</returns>
		CounterpartyContractInfoForEdo ConvertCounterpartyContractToCounterpartyContractInfoForEdo(
			CounterpartyContractEntity contract, DateTime dateTime);
	}
}
