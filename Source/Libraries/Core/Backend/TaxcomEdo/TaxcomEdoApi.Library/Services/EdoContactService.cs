using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Infrastructure;
using Microsoft.Extensions.Options;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdoApi.Library.Config;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoContactService : EdoLoginService, IEdoContactService
	{
		private readonly HttpClient _httpClient;
		private readonly TaxcomEdoApiOptions _options;

		public EdoContactService(
			HttpClient httpClient,
			IEdoAuthorizationService edoAuthorizationService,
			IOptions<TaxcomEdoApiOptions> options) : base(edoAuthorizationService)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
		}
		
		public async Task<EdoContactList> CheckCounterpartyAsync(string inn, string kpp, string login, string password)
		{
			var key = await Login(login, password);
			var contactList = EdoContactList.CreateForCheckCounterparty(inn, kpp);
			
			return await CheckCounterpartyAsync(key, contactList.ToXmlString());
		}
		
		public async Task<EdoContactList> CheckCounterpartyAsync(string inn, string kpp, byte[] certificateData)
		{
			var key = await CertificateLogin(certificateData);
			var contactList = EdoContactList.CreateForCheckCounterparty(inn, kpp);
			
			return await CheckCounterpartyAsync(key, contactList.ToXmlString());
		}

		public async Task<bool> SendContactsAsync(string contactList, string login, string password)
		{
			var key = await Login(login, password);
			return await SendContactsAsync(key, contactList);
		}

		public async Task<bool> SendContactsAsync(string contactList, byte[] certificateData)
		{
			var key = await CertificateLogin(certificateData);
			return await SendContactsAsync(key, contactList);
		}

		public async Task<bool> AcceptContactAsync(string clientEdoAccountId)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(clientEdoAccountId, "id")
				.ToString();
			
			var response = await _httpClient.GetAsync(_options.ContactsUri.AcceptContactUri + query);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> RejectContactAsync(string clientEdoAccountId, string comment = null)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(clientEdoAccountId, "id");

			if(!string.IsNullOrWhiteSpace(comment))
			{
				query.AddParameter("comment", comment);
			}
			
			var response = await _httpClient.GetAsync(_options.ContactsUri.RejectContactUri + query.ToString());
			return response.IsSuccessStatusCode;
		}

		public async Task<EdoContactList> GetContactListUpdatesAsync(string edoAccountId)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(edoAccountId, "id")
				.ToString();
			
			var response = await _httpClient.GetAsync(_options.ContactsUri.RejectContactUri + query);
			var contacts =
				(await response.Content.ReadAsStringAsync())
				.DeserializeXmlString<EdoContactList>();
			
			return contacts;
		}

		public async Task<EdoContactList> GetContactsAsync(string edoAccountId)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(edoAccountId, "id")
				.ToString();
			
			var response = await _httpClient.GetAsync(_options.ContactsUri.RejectContactUri + query);
			var contacts =
				(await response.Content.ReadAsStringAsync())
				.DeserializeXmlString<EdoContactList>();
			
			return contacts;
		}
		
		private async Task<bool> SendContactsAsync(string key, string contactList)
		{
			var content = PrepareContactsContent(Encoding.UTF8.GetBytes(contactList), key);
			var response = await _httpClient.PostAsync(_options.ContactsUri.SendContactsUri, content);
			
			return response.IsSuccessStatusCode;
		}
		
		private async Task<EdoContactList> CheckCounterpartyAsync(string key, string contactList)
		{
			var content = PrepareContactsContent(Encoding.UTF8.GetBytes(contactList), key);
			var response = await _httpClient.PostAsync(_options.ContactsUri.CheckContragentUri, content);

			return response.IsSuccessStatusCode ? (await response.Content.ReadAsStringAsync()).DeserializeXmlString<EdoContactList>() : null;
		}
		
		private ByteArrayContent PrepareContactsContent(byte[] contacts, string assistantKey)
		{
			var content = new ByteArrayContent(contacts);
			content.Headers.Add("Assistant-Key", assistantKey);
			content.Headers.ContentLength = contacts.Length;
			return content;
		}
	}

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
		/// <param name="inn">ИНН</param>
		/// <param name="kpp">КПП</param>
		/// <param name="login">Логин</param>
		/// <param name="password">Пароль</param>
		/// <returns></returns>
		Task<EdoContactList> CheckCounterpartyAsync(string inn, string kpp, string login, string password);
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
		/// <param name="inn">ИНН</param>
		/// <param name="kpp">КПП</param>
		/// <param name="certificateData">Подпись</param>
		/// <returns></returns>
		Task<EdoContactList> CheckCounterpartyAsync(string inn, string kpp, byte[] certificateData);
		/// <summary>
		/// Отправка приглашения зарегистрированным в системе Такском-Доклайнз контрагентам через систему Такском-Доклайнз.
		/// </summary>
		/// <param name="contactList">Список приглашений</param>
		/// <param name="login">Логин</param>
		/// <param name="password">Пароль</param>
		/// <returns></returns>
		Task<bool> SendContactsAsync(string contactList, string login, string password);
		/// <summary>
		/// Отправка приглашения зарегистрированным в системе Такском-Доклайнз контрагентам через систему Такском-Доклайнз.
		/// </summary>
		/// <param name="contactList">Список приглашений</param>
		/// <param name="certificateData">Подпись</param>
		/// <returns></returns>
		Task<bool> SendContactsAsync(string contactList, byte[] certificateData);
		/// <summary>
		/// Подтверждение согласия на обмен электронными документами в ответ на полученное от контрагента приглашение
		/// </summary>
		/// <param name="clientEdoAccountId">Номер кабинета клиента, приглашение которого принимается</param>
		/// <returns></returns>
		Task<bool> AcceptContactAsync(string clientEdoAccountId);
		/// <summary>
		/// Отклонение приглашения к обмену документами или для запрещения обмена документами с одним из контрагентов
		/// </summary>
		/// <param name="clientEdoAccountId">Номер кабинета клиента</param>
		/// <param name="comment">Причина отклонения</param>
		/// <returns></returns>
		Task<bool> RejectContactAsync(string clientEdoAccountId, string comment = null);
		/// <summary>
		/// Получение обновления для списка статусов приглашений,
		/// а также информации о подразделениях и сотрудниках организации с включенной опцией "Показывать контрагентам при заполнении данных получателя".
		/// Вызывать метод GetContactListUpdates следует не чаще, чем 1 раз в минуту
		/// </summary>
		/// <param name="edoAccountId">Кабинет организации, от которой идет запрос</param>
		/// <returns></returns>
		Task<EdoContactList> GetContactListUpdatesAsync(string edoAccountId);
		/// <summary>
		/// Получение из системы Такском-Доклайнз актуального списка статусов приглашений к обмену электронными документами,
		/// отправленных или полученных данным абонентом, а также информации о подразделениях и сотрудниках организации с включенной опцией "Показывать контрагентам при заполнении данных получателя".
		/// Можно вызывать не чаще, чем один раз в минуту. Нарушение этого условия приведёт к ошибке 2209 "Превышена частота обращений в установленный интервал времени".
		/// Метод GetContacts позволяет получить не более 1000 контактов.
		/// Если контактов больше, то следует использовать метод <see cref="GetContactListUpdatesAsync"/>
		/// </summary>
		/// <param name="edoAccountId">Кабинет организации, от которой идет запрос</param>
		/// <returns></returns>
		Task<EdoContactList> GetContactsAsync(string edoAccountId);
	}
}
