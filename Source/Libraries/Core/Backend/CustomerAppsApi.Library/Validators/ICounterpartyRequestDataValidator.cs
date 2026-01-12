using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Dto.Edo;
using CustomerAppsApi.Library.Dto.Phones;

namespace CustomerAppsApi.Library.Validators
{
	public interface ICounterpartyRequestDataValidator
	{
		string CounterpartyContactInfoDtoValidate(CounterpartyContactInfoDto counterpartyContactInfoDto);
		string CounterpartyDtoValidate(CounterpartyDto counterpartyDto);
		/// <summary>
		/// Проверка данных запроса получения компаний по ИНН
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns></returns>
		string LegalCustomersByInnValidate(LegalCustomersByInnRequest dto);
		/// <summary>
		/// Проверка данных регистрации нового юр лица
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns></returns>
		string RegisteringLegalCustomerValidate(RegisteringLegalCustomerDto dto);
		string ConnectingLegalCustomerValidate(ConnectingLegalCustomerDto dto);
		string GetPhonesConnectedToLegalCustomerValidate(GetPhonesConnectedToLegalCustomerDto dto);
		string UpdateConnectToLegalCustomerByPhoneValidate(UpdateConnectToLegalCustomerByPhoneDto dto);
		string ConnectingNewPhoneToLegalCustomerValidate(ConnectingNewPhoneToLegalCustomerDto dto);
		string GetNaturalCounterpartyLegalCustomersDtoValidate(GetNaturalCounterpartyLegalCustomersDto dto);
		/// <summary>
		/// Проверка данных запроса получения юр лиц с активной почтой
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns></returns>
		string CompanyWithActiveEmailValidate(CompanyWithActiveEmailRequest dto);
		/// <summary>
		/// Проверка данных запроса получения информации об юр лице
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		string CompanyInfoRequestDataValidate(CompanyInfoRequest dto);
		/// <summary>
		/// Проверка данных запроса подключения почты для связки аккаунта юр лица
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns></returns>
		string LinkingEmailToLegalCounterpartyValidate(LinkingLegalCounterpartyEmailToExternalUser dto);
		/// <summary>
		/// Проверка данных получения контактов юр лица
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns></returns>
		string GetLegalCustomerContactsValidate(LegalCounterpartyContactListRequest dto);
		/// <summary>
		/// Проверка данных по проверке пароля
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns></returns>
		string CheckPasswordValidate(CheckPasswordRequest dto);
		/// <summary>
		/// Проверка данных по добавлению телефона
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns>Найденные ошибки</returns>
		string AddPhoneToCounterpartyValidate(AddingPhoneNumberDto dto);
		/// <summary>
		/// Проверка данных по обновлению цели покупки воды
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns>Найденные ошибки</returns>
		string UpdateCounterpartyPurposeOfPurchaseValidate(UpdatingCounterpartyPurposeOfPurchase dto);
		/// <summary>
		/// Проверка данных при добавлении ЭДО аккаунта
		/// </summary>
		/// <param name="dto">Данные запроса</param>
		/// <returns>Найденные ошибки</returns>
		string AddEdoAccountValidate(AddingEdoAccount dto);
		/// <summary>
		/// Проверка данных при получении операторов ЭДО
		/// </summary>
		/// <param name="request">Данные запроса</param>
		/// <returns>Найденные ошибки</returns>
		string GetOperatorsValidate(GetEdoOperatorsRequest request);
	}
}
