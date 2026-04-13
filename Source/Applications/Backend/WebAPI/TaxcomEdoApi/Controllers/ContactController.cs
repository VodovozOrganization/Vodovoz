using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Responses;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
	public class ContactController : ControllerBase
	{
		private readonly ILogger<ContactController> _logger;
		private readonly IEdoContactService _edoContactService;
		private readonly X509Certificate2 _certificate;

		public ContactController(
			ILogger<ContactController> logger,
			IEdoContactService edoContactService,
			X509Certificate2 certificate)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoContactService = edoContactService ?? throw new ArgumentNullException(nameof(edoContactService));
			_certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
		}
		
		/// <summary>
		/// Получение обновлений списка статусов приглашений
		/// </summary>
		/// <param name="dateTime">Дата и время начала поиска</param>
		/// <param name="contactState">Статус контакта</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpGet]
		public async Task<TaxcomResponse<EdoContactList>> GetContactListUpdates(
			DateTime dateTime, EdoContactStateCode? contactState, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получаем обновленный список контактов c {DateTime}...", dateTime.ToShortDateString());
			
			try
			{
				var response = await _edoContactService.GetContactListUpdatesAsync(
					dateTime, contactState, _certificate.RawData, cancellationToken);
				
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении обновлений для списка контактов");
				
				return TaxcomResponse<EdoContactList>.Error(
					"Произошла ошибка при получении обновлений для списка контактов. " +
					"Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		/// <summary>
		/// Отправка приглашения зарегистрированным в системе Такском-Доклайнз контрагентам через систему Такском-Доклайнз.
		/// </summary>
		/// <param name="contactList">Список клиентов для приглашений</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<TaxcomResponse> SendContacts(EdoContactList contactList, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Отправляем приглашение на ЭДО...");
			
			try
			{
				var response = await _edoContactService.SendContactsAsync(contactList, _certificate.RawData, cancellationToken);
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при отправке приглашения");
				
				return TaxcomResponse.Error(
					"Произошла неизвестная ошибка при отправке приглашения на ЭДО." +
					" Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		/// <summary>
		///  Подтверждение согласия на обмен электронными документами в ответ на полученное от контрагента приглашение
		/// </summary>
		/// <param name="clientEdoAccountId">Идентификатор участника ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpGet]
		public async Task<TaxcomResponse> AcceptContact(string clientEdoAccountId, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Принимаем входящее приглашение по ЭДО от клиента с аккаунтом {ClientEdoAccountId}", clientEdoAccountId);
			
			try
			{
				var response = await _edoContactService.AcceptContactAsync(clientEdoAccountId, _certificate.RawData, cancellationToken);
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при принятии приглашения от клиента с аккаунтом {ClientEdoAccountId}", clientEdoAccountId);
				return TaxcomResponse.Error(
					$"Произошла неизвестная ошибка при принятии приглашения от клиента с аккаунтом {clientEdoAccountId}." +
					" Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		/// <summary>
		/// Отклонение приглашения к обмену документами или для запрещения обмена документами с одним из контрагентов.
		/// </summary>
		/// <param name="clientEdoAccountId">Идентификатор участника ЭДО</param>
		/// <param name="comment">Комментарий</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpGet]
		public async Task<TaxcomResponse> RejectContact(string clientEdoAccountId, string comment, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Отклонение приглашения по ЭДО от клиента с аккаунтом {ClientEdoAccountId}", clientEdoAccountId);
			
			try
			{
				var response = await _edoContactService.RejectContactAsync(clientEdoAccountId, _certificate.RawData, comment, cancellationToken);
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при отклонении приглашения от клиента с аккаунтом {ClientEdoAccountId}", clientEdoAccountId);
				return TaxcomResponse.Error(
					$"Произошла неизвестная ошибка при отклонении приглашения от клиента с аккаунтом {clientEdoAccountId}." +
					" Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
		
		/// <summary>
		/// Получение из системы Такском-Доклайнз актуального списка статусов приглашений к обмену электронными документами,
		/// отправленных или полученных данным абонентом,
		/// а также информации о подразделениях и сотрудниках организации с включенной опцией "Показывать контрагентам при заполнении данных получателя"
		/// <para>
		/// Метод GetContacts  можно вызывать не чаще, чем один раз в минуту.
		/// Нарушение этого условия приведёт к ошибке 2209 "Превышена частота обращений в установленный интервал времени".
		///	Метод GetContacts позволяет получить не более 1000 контактов. Если контактов больше, то следует использовать метод
		/// <see cref="GetContactListUpdates"/>
		/// </para>
		/// </summary>
		/// <param name="clientEdoAccountId"></param>
		/// <param name="comment"></param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpGet]
		public async Task<IActionResult> GetContacts(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получение списка контактов");
			
			try
			{
				var response = await _edoContactService.GetContactsAsync(_certificate.RawData, cancellationToken);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении списка контактов");
				return Problem();
			}
		}
		
		/// <summary>
		/// Позволяет по паре ИНН/КПП или по ограниченному списку ИНН/КПП,
		/// но не более 20 пар ИНН/КПП в одном запросе и с интервалом не менее 5 секунд получить информацию,
		/// является ли организация абонентом Такском по ЭДО. Если у ООО "Такском" есть такой (такие) зарегистрированный абонент (абоненты),
		/// то возвращается идентификатор участника ЭДО и информация о его активности (активен/неактивен).
		/// Если на одну пару ИНН/КПП зарегистрировано более одного участника ЭДО, то в результате возвращается список всех зарегистрированных идентификаторов.
		/// Абонент считается активным если за предыдущие 6 месяцев зафиксирована его авторизация в кабинете. В ином случае абонент считается неактивным.
		///	Если с указанным ИНН/КПП зарегистрирован роуминговый кабинет (т.е. кабинет с префиксом, отличным от 2AL),
		/// то в результате возвращается информация об этом кабинете, но без признака активности.
		/// </summary>
		/// <param name="contactList">Список на проверку</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<TaxcomResponse<EdoContactList>> CheckCounterparty(EdoContactList contactList, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Проверка наличия данных о клиентах в Такскоме");
			
			try
			{
				var response = await _edoContactService.CheckCounterpartyAsync(contactList, _certificate.RawData, cancellationToken);
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при проверке наличия данных о клиентах в Такскоме");
				
				return TaxcomResponse<EdoContactList>.Error(
					"Произошла неизвестная ошибка при проверке наличия данных о клиентах в Такскоме." +
					" Попробуйте позднее или обратитесь в отдел разработки");
			}
		}
	}
}
