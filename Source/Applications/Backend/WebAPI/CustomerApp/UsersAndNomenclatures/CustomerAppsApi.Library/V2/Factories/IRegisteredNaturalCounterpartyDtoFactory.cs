using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface IRegisteredNaturalCounterpartyDtoFactory
	{
		/// <summary>
		/// Создание информации по созданному клиенту физ лицу
		/// </summary>
		/// <param name="externalCounterparty">Пользователь ИПЗ</param>
		/// <returns></returns>
		RegisteredNaturalCounterpartyDto CreateNewRegisteredNaturalCounterpartyDto(ExternalCounterparty externalCounterparty);
	}
}
