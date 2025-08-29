using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Taxcom.Client.Api;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdoApi.Library.Services;

namespace TaxcomEdoApi.Controllers
{
	public class ContactController : ControllerBase
	{
		private readonly ILogger<ContactController> _logger;
		private readonly IEdoContactService _edoContactService;

		public ContactController(
			ILogger<ContactController> logger,
			IEdoContactService edoContactService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoContactService = edoContactService ?? throw new ArgumentNullException(nameof(edoContactService));
		}
		
		[HttpGet]
		public IActionResult GetContactListUpdates(DateTime? lastCheckContactsUpdates, EdoContactStateCode? contactState)
		{
			_logger.LogInformation("Получаем обновленный список контактов...");

			ContactStatus? contactStatus = null;

			if(contactState.HasValue)
			{
				if(Enum.TryParse(contactState.ToString(), out ContactStatus parsedContactStatus))
				{
					contactStatus = parsedContactStatus;
				}
			}
			
			try
			{
				var response = _client.GetContactListUpdates(lastCheckContactsUpdates, contactStatus);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении обновлений для списка контактов");
				return Problem();
			}
		}
		
		[HttpPost]
		public IActionResult SendContact()
		{
			_logger.LogInformation("Отправляем приглашение на ЭДО...");

			ContactStatus? contactStatus = null;

			if(contactState.HasValue)
			{
				if(Enum.TryParse(contactState.ToString(), out ContactStatus parsedContactStatus))
				{
					contactStatus = parsedContactStatus;
				}
			}
			
			try
			{
				var response = _client.GetContactListUpdates(lastCheckContactsUpdates, contactStatus);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении обновлений для списка контактов");
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult AcceptContact(string clientEdoAccountId)
		{
			_logger.LogInformation("Принимаем входящее приглашение по ЭДО от клиента с аккаунтом {ClientEdoAccountId}", clientEdoAccountId);
			
			try
			{
				var response = _edoContactService.AcceptContactAsync(clientEdoAccountId);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при принятии приглашения от клиента с аккаунтом {ClientEdoAccountId}", clientEdoAccountId);
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult RejectContact(string clientEdoAccountId, string comment)
		{
			_logger.LogInformation("Отклонение приглашения по ЭДО от клиента с аккаунтом {ClientEdoAccountId}", clientEdoAccountId);
			
			try
			{
				var response = _edoContactService.RejectContactAsync(clientEdoAccountId);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при отклонении приглашения от клиента с аккаунтом {ClientEdoAccountId}", clientEdoAccountId);
				return Problem();
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
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetContacts()
		{
			_logger.LogInformation("Получение списка контактов");
			
			try
			{
				var response = _edoContactService.GetContacts(clientEdoAccountId);
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
		/// <param name="clientEdoAccountId"></param>
		/// <param name="comment"></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult CheckContragent(object innKpp)
		{
			_logger.LogInformation("Проверка наличия данных о клиентах в Такскоме");
			
			try
			{
				var response = _edoContactService.CheckContragent(clientEdoAccountId);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при проверке наличия данных о клиентах в Такскоме");
				return Problem();
			}
		}
	}
}
