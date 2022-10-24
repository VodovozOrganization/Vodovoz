using EdoApi.Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using TISystems.TTC.CRM.BE.Serialization;

namespace EdoWebApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class EdoController : ControllerBase
	{
		private readonly ILogger<EdoController> _logger;
		private readonly IAuthorizationService _authorizationService;
		private readonly IContactListService _contactListService;
		private readonly ITrueApiService _trueApiService;

		public EdoController(
			ILogger<EdoController> logger,
			IAuthorizationService authorizationService,
			IContactListService contactListService,
			ITrueApiService trueApiService)
		{
			//_logger = logger;
			_authorizationService = authorizationService;
			_contactListService = contactListService;
			_trueApiService = trueApiService;
		}

		[HttpGet]
		[Route("/Login")]
		public async Task<string> Login() => await _authorizationService.Login();

		[HttpGet]
		[Route("/SendContacts")] // Отправить приглашение
		public async Task SendContacts(string inn, string kpp, string email) // post dto?
		{
			var key = await Login();

			byte[] requestBytes;
			var invitationsList = new ContactList
			{
				Contacts = new[]
				{
					new ContactListItem
					{
						Inn = inn,
						Kpp = kpp,
						Email = email,
						Comment = "Это Иванов Иван Иванович, давайте обмениваться документами электронно",
					},
				}
			};

			using(var ms = new MemoryStream())
			{
				ContactListSerializer.SerializeContactList(invitationsList, ms);
				requestBytes = ms.ToArray();
			}

			await _contactListService.SendContactsAsync(requestBytes, key);
		}

		//2022-10-20T14:16:50 - последнее время запроса ( интервал минута), по 100 штук, если вернулась ошибка повторить  проверять сколько прошло время, dto с ContactList + Errror

		[HttpGet]
		[Route("/GetContactListUpdates")] //Проверить согласие клиента
		public async Task<ContactList> GetContactListUpdates(DateTime dateLastRequest, ContactStateCode? status)
		{
			//dateLastRequest = DateTime.Now.AddDays(-5);
			var key = await Login();
			return await _contactListService.GetContactListUpdatesAsync(dateLastRequest, key, status);
		}

		[HttpGet]
		[Route("/CheckContragent")] // Проверить в такском
		public async Task<ContactList> CheckContragent(string inn, string kpp)
		{
			var key = await Login();

			byte[] requestBytes;
			var invitationsList = new ContactList
			{
				Contacts = new[]
				{
					new ContactListItem
					{
						Inn = inn,    //7820325792
                        Kpp = kpp    //782001001
                    }
				}
			};

			using(var ms = new MemoryStream())
			{
				ContactListSerializer.SerializeContactList(invitationsList, ms);
				requestBytes = ms.ToArray();
			}

			return await _contactListService.CheckContragentAsync(requestBytes, key);
		}

		[HttpGet]
		[Route("/CheckCounterpartyInTrueApi")] // Проверить в Чесном знаке
		public async Task<bool> CheckCounterpartyInTrueApi(string inn, string productGroup)
		{
			return await _trueApiService.GetCounterpartyRegisteredInTrueApi(inn, productGroup);
		}
	}
}
