using System;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Responses;

namespace TaxcomEdoApi.Library.Services.Interfaces;

/// <summary>
/// Управление списком клиентов В ЭДО
/// </summary>
public interface IEdoContactService
{
	/// <summary>
	/// Позволяет по паре ИНН/КПП или по ограниченному списку ИНН/КПП,
	/// но не более 20 пар ИНН/КПП в одном запросе и с интервалом не менее 5 секунд получить информацию,
	/// является ли организация абонентом Такском по ЭДО. Если у ООО "Такском" есть такой (такие) зарегистрированный абонент (абоненты),
	/// то возвращается идентификатор участника ЭДО и информация о его активности (активен/неактивен).
	/// Если на одну пару ИНН/КПП зарегистрировано более одного участника ЭДО,
	/// то в результате возвращается список всех зарегистрированных идентификаторов.
	/// Абонент считается активным если за предыдущие 6 месяцев зафиксирована его авторизация в кабинете.
	/// В ином случае абонент считается неактивным.
	/// Если с указанным ИНН/КПП зарегистрирован роуминговый кабинет (т.е. кабинет с префиксом, отличным от 2AL),
	/// то в результате возвращается информация об этом кабинете, но без признака активности.
	/// </summary>
	/// <param name="contactList">Список ИНН/КПП</param>
	/// <param name="login">Логин</param>
	/// <param name="password">Пароль</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse<EdoContactList>> CheckCounterpartyAsync(
		EdoContactList contactList, string login, string password, CancellationToken cancellationToken = default);
	/// <summary>
	/// Позволяет по паре ИНН/КПП или по ограниченному списку ИНН/КПП,
	/// но не более 20 пар ИНН/КПП в одном запросе и с интервалом не менее 5 секунд получить информацию,
	/// является ли организация абонентом Такском по ЭДО. Если у ООО "Такском" есть такой (такие) зарегистрированный абонент (абоненты),
	/// то возвращается идентификатор участника ЭДО и информация о его активности (активен/неактивен).
	/// Если на одну пару ИНН/КПП зарегистрировано более одного участника ЭДО,
	/// то в результате возвращается список всех зарегистрированных идентификаторов.
	/// Абонент считается активным если за предыдущие 6 месяцев зафиксирована его авторизация в кабинете.
	/// В ином случае абонент считается неактивным.
	/// Если с указанным ИНН/КПП зарегистрирован роуминговый кабинет (т.е. кабинет с префиксом, отличным от 2AL),
	/// то в результате возвращается информация об этом кабинете, но без признака активности.
	/// </summary>
	/// <param name="contactList">Список ИНН/КПП</param>
	/// <param name="certificateData">Подпись</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse<EdoContactList>> CheckCounterpartyAsync(
		EdoContactList contactList, byte[] certificateData, CancellationToken cancellationToken = default);
	/// <summary>
	/// Отправка приглашения зарегистрированным в системе Такском-Доклайнз контрагентам через систему Такском-Доклайнз.
	/// </summary>
	/// <param name="contactsList">Список приглашений</param>
	/// <param name="login">Логин</param>
	/// <param name="password">Пароль</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse> SendContactsAsync(EdoContactList contactsList, string login, string password, CancellationToken cancellationToken = default);
	/// <summary>
	/// Отправка приглашения зарегистрированным в системе Такском-Доклайнз контрагентам через систему Такском-Доклайнз.
	/// </summary>
	/// <param name="contactsList">Список приглашений</param>
	/// <param name="certificateData">Подпись</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse> SendContactsAsync(EdoContactList contactsList, byte[] certificateData, CancellationToken cancellationToken = default);
	/// <summary>
	/// Подтверждение согласия на обмен электронными документами в ответ на полученное от контрагента приглашение
	/// </summary>
	/// <param name="clientEdoAccountId">Номер кабинета клиента, приглашение которого принимается</param>
	/// <param name="login">Логин</param>
	/// <param name="password">Пароль</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse> AcceptContactAsync(string clientEdoAccountId, string login, string password, CancellationToken cancellationToken = default);
	/// <summary>
	/// Подтверждение согласия на обмен электронными документами в ответ на полученное от контрагента приглашение
	/// </summary>
	/// <param name="clientEdoAccountId">Номер кабинета клиента, приглашение которого принимается</param>
	/// <param name="certificateData">Подпись</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse> AcceptContactAsync(string clientEdoAccountId, byte[] certificateData, CancellationToken cancellationToken = default);
	/// <summary>
	/// Отклонение приглашения к обмену документами или для запрещения обмена документами с одним из контрагентов
	/// </summary>
	/// <param name="clientEdoAccountId">Номер кабинета клиента</param>
	/// <param name="comment">Причина отклонения</param>
	/// <param name="login">Логин</param>
	/// <param name="password">Пароль</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse> RejectContactAsync(
		string clientEdoAccountId, string login, string password, string comment = null, CancellationToken cancellationToken = default);
	/// <summary>
	/// Отклонение приглашения к обмену документами или для запрещения обмена документами с одним из контрагентов
	/// </summary>
	/// <param name="clientEdoAccountId">Номер кабинета клиента</param>
	/// <param name="certificateData">Подпись</param>
	/// <param name="comment">Причина отклонения</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse> RejectContactAsync(
		string clientEdoAccountId, byte[] certificateData, string comment = null, CancellationToken cancellationToken = default);
	/// <summary>
	/// Получение обновления для списка статусов приглашений,
	/// а также информации о подразделениях и сотрудниках организации с включенной опцией "Показывать контрагентам при заполнении данных получателя".
	/// Вызывать метод GetContactListUpdates следует не чаще, чем 1 раз в минуту
	/// </summary>
	/// <param name="dateTime">Время начала выборки изменений</param>
	/// <param name="stateCode">Необязательный параметр. Используется для фильтрации приглашений по статусу</param>
	/// <param name="login">Логин</param>
	/// <param name="password">Пароль</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse<EdoContactList>> GetContactListUpdatesAsync(
		DateTime dateTime, EdoContactStateCode? stateCode, string login, string password, CancellationToken cancellationToken = default);
	/// <summary>
	/// Получение обновления для списка статусов приглашений,
	/// а также информации о подразделениях и сотрудниках организации с включенной опцией "Показывать контрагентам при заполнении данных получателя".
	/// Вызывать метод GetContactListUpdates следует не чаще, чем 1 раз в минуту
	/// </summary>
	/// <param name="dateTime">Время начала выборки изменений</param>
	/// <param name="stateCode">Необязательный параметр. Используется для фильтрации приглашений по статусу</param>
	/// <param name="certificateData">Подпись</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns></returns>
	Task<TaxcomResponse<EdoContactList>> GetContactListUpdatesAsync(
		DateTime dateTime, EdoContactStateCode? stateCode, byte[] certificateData, CancellationToken cancellationToken = default);
	/// <summary>
	/// Получение из системы Такском-Доклайнз актуального списка статусов приглашений к обмену электронными документами,
	/// отправленных или полученных данным абонентом, а также информации о подразделениях и сотрудниках организации с включенной опцией "Показывать контрагентам при заполнении данных получателя".
	/// Можно вызывать не чаще, чем один раз в минуту. Нарушение этого условия приведёт к ошибке 2209 "Превышена частота обращений в установленный интервал времени".
	/// Метод GetContacts позволяет получить не более 1000 контактов.
	/// Если контактов больше, то следует использовать метод <see cref="GetContactListUpdatesAsync"/>
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <param name="certificateData">Подпись</param>
	/// <returns></returns>
	Task<TaxcomResponse<EdoContactList>> GetContactsAsync(byte[] certificateData, CancellationToken cancellationToken = default);
	/// <summary>
	/// Получение из системы Такском-Доклайнз актуального списка статусов приглашений к обмену электронными документами,
	/// отправленных или полученных данным абонентом, а также информации о подразделениях и сотрудниках организации с включенной опцией "Показывать контрагентам при заполнении данных получателя".
	/// Можно вызывать не чаще, чем один раз в минуту. Нарушение этого условия приведёт к ошибке 2209 "Превышена частота обращений в установленный интервал времени".
	/// Метод GetContacts позволяет получить не более 1000 контактов.
	/// Если контактов больше, то следует использовать метод <see cref="GetContactListUpdatesAsync"/>
	/// </summary>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <param name="login">Логин</param>
	/// <param name="password">Пароль</param>
	/// <returns></returns>
	Task<TaxcomResponse<EdoContactList>> GetContactsAsync(string login, string password, CancellationToken cancellationToken = default);
}
