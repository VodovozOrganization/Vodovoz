using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface ICounterpartyModelFactory
	{
		/// <summary>
		/// Создание ошибочной идентификации клиента
		/// </summary>
		/// <param name="error">Описание ошибки</param>
		/// <returns></returns>
		CounterpartyIdentificationDto CreateErrorCounterpartyIdentificationDto(string error);
		/// <summary>
		/// Создание идентификации клиента с не найденным клиента в базе
		/// </summary>
		/// <returns></returns>
		CounterpartyIdentificationDto CreateNotFoundCounterpartyIdentificationDto();
		/// <summary>
		/// Создание идентификации на ручную обработку
		/// </summary>
		/// <returns></returns>
		CounterpartyIdentificationDto CreateNeedManualHandlingCounterpartyIdentificationDto();
		/// <summary>
		/// Создание данных на ручную обработку
		/// </summary>
		/// <param name="counterpartyContactInfoDto">Данные по контакту</param>
		/// <param name="counterpartyFrom">Откуда клиент</param>
		/// <returns></returns>
		CounterpartyManualHandlingDto CreateNeedManualHandlingCounterpartyDto(
			CounterpartyContactInfoDto counterpartyContactInfoDto, CounterpartyFrom counterpartyFrom);
		/// <summary>
		/// Создание успешной идентификации
		/// </summary>
		/// <param name="externalCounterparty">Данные пользователя</param>
		/// <returns></returns>
		CounterpartyIdentificationDto CreateSuccessCounterpartyIdentificationDto(ExternalCounterparty externalCounterparty);
		/// <summary>
		/// Создание идентификации с зарегистрированным клиентом
		/// </summary>
		/// <param name="externalCounterparty">Данные пользователя</param>
		/// <returns></returns>
		CounterpartyIdentificationDto CreateRegisteredCounterpartyIdentificationDto(ExternalCounterparty externalCounterparty);
		/// <summary>
		/// Создание данных по регистрации клиента с ошибкой
		/// </summary>
		/// <param name="error">Описание ошибки</param>
		/// <returns></returns>
		CounterpartyRegistrationDto CreateErrorCounterpartyRegistrationDto(string error);
		/// <summary>
		/// Создание данных по регистрации клиента
		/// </summary>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <returns></returns>
		CounterpartyRegistrationDto CreateRegisteredCounterpartyRegistrationDto(int counterpartyId);
		/// <summary>
		/// Создание информации по обновлению данных клиента с ошибкой
		/// </summary>
		/// <param name="error">Описание ошибки</param>
		/// <returns></returns>
		CounterpartyUpdateDto CreateErrorCounterpartyUpdateDto(string error);
		/// <summary>
		/// Создание информации по обновлению данных клиента, когда он не найден в системе
		/// </summary>
		/// <returns></returns>
		CounterpartyUpdateDto CreateNotFoundCounterpartyUpdateDto();
	}
}
