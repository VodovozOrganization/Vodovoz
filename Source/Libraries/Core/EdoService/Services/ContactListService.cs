using EdoService.Library.Converters;
using EdoService.Library.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Settings.Edo;

namespace EdoService.Library.Services
{
	public class ContactListService : IContactListService
	{
		private readonly IAuthorizationService _authorizationService;
		private readonly IEdoSettings _edoSettings;
		private readonly IContactStateConverter _contactStateConverter;
		private readonly IGenericRepository<TaxcomEdoSettings> _edoSettingsRepository;
		private static HttpClient _httpClient;
		private readonly IEdoLogger _edoLogger;

		public ContactListService(
			IEdoLogger edoLogger,
			IAuthorizationService authorizationService,
			IEdoSettings edoSettings,
			IContactStateConverter contactStateConverter,
			IGenericRepository<TaxcomEdoSettings> edoSettingsRepository)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_contactStateConverter = contactStateConverter ?? throw new ArgumentNullException(nameof(contactStateConverter));
			_edoSettingsRepository = edoSettingsRepository ?? throw new ArgumentNullException(nameof(edoSettingsRepository));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

			_edoLogger = edoLogger ?? throw new ArgumentNullException(nameof(edoLogger));

			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri(edoSettings.TaxcomBaseAddressUri)
			};

			_httpClient.DefaultRequestHeaders.Add("Integrator-Id", _edoSettings.TaxcomIntegratorId);
		}

		private string Encode(string str)
		{
			var bytes = Encoding.GetEncoding(1252).GetBytes(str);
			return Encoding.GetEncoding(1251).GetString(bytes, 0, bytes.Length);
		}

		private ByteArrayContent PrepareContactsContent(byte[] contacts, string assistantKey)
		{
			var content = new ByteArrayContent(contacts);
			content.Headers.Add("Assistant-Key", assistantKey);
			content.Headers.ContentLength = contacts.Length;
			return content;
		}

		public async Task<string> Login(string login, string password) => await _authorizationService.Login(login, password);

		public async Task<ContactList> CheckContragentAsync(IUnitOfWork uow, int organizationId, string inn, string kpp)
		{
			var key = await GetSettingsAndLogin(uow, organizationId);

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

			var content = PrepareContactsContent(requestBytes, key);
			var response = await _httpClient.PostAsync(_edoSettings.TaxcomCheckContragentUri, content);
			var result = await response.Content.ReadAsStreamAsync();

			if(response.IsSuccessStatusCode)
			{
				return ContactListSerializer.DeserializeContactList(result);
			}

			_edoLogger.LogError(response);

			return null;
		}

		private async Task<string> GetSettingsAndLogin(IUnitOfWork uow, int organizationId)
		{
			var edoSettings = _edoSettingsRepository
				.Get(uow, x => x.OrganizationId == organizationId)
				.FirstOrDefault();
			
			if(edoSettings == null)
			{
				throw new InvalidOperationException($"Не заполнены настройки по ЭДО для организации с Id {organizationId}");
			}

			return await Login(edoSettings.Login, edoSettings.Password);
		}

		public async Task<ResultDto> SendContactsAsync(
			IUnitOfWork uow,
			int organizationId,
			string inn,
			string kpp,
			string email,
			string edxClientId,
			string organizationName)
		{
			var invitationsList = new ContactList
			{
				Contacts = new[]
				{
					new ContactListItem
					{
						Inn = inn,
						Kpp = kpp,
						Email = email,
						EdxClientId = Regex.Replace(edxClientId, @"\s+", string.Empty),
						Comment = $"{organizationName} приглашает Вас к электронному обмену документами"
					}
				}
			};

			return await SendContactsAsync(uow, organizationId, invitationsList);
		}

		public async Task<ResultDto> SendContactsForManualInvitationAsync(
			IUnitOfWork uow,
			int organizationId,
			string inn,
			string kpp,
			string organizationName,
			string operatorId,
			string email,
			string scanFileName,
			byte[] scanFile)
		{
			var invitationsList = new ContactList
			{
				Contacts = new[]
				{
					new ContactListItem
					{
						Inn = inn,
						Kpp = kpp,
						Name = organizationName,
						Email = email,
						OperatorId = operatorId,
						ScanFilename = scanFileName,
						Scan = Convert.ToBase64String(scanFile),
						Comment = $"Компания {organizationName} приглашает Вас к электронному обмену документами"
					}
				}
			};

			return await SendContactsAsync(uow, organizationId, invitationsList);
		}

		public async Task<ResultDto> SendContactsAsync(IUnitOfWork uow, int organizationId, ContactList invitationsList)
		{
			var key = await GetSettingsAndLogin(uow, organizationId);

			byte[] requestBytes;

			using(var ms = new MemoryStream())
			{
				ContactListSerializer.SerializeContactList(invitationsList, ms);
				requestBytes = ms.ToArray();
			}

			var content = PrepareContactsContent(requestBytes, key);

			var response = await _httpClient.PostAsync(_edoSettings.TaxcomSendContactsUri, content);

			if(response.IsSuccessStatusCode)
			{
				return new ResultDto
				{
					IsSuccess = true
				};
			}

			_edoLogger.LogError(response);

			return new ResultDto
			{
				IsSuccess = false,
				ErrorMessage = $"Приглашение не отправлено!\n{response.StatusCode}, {Encode(response.ReasonPhrase)}"
			};
		}

		public ConsentForEdoStatus ConvertStateToConsentForEdoStatus(ContactStateCode stateCode) =>
			_contactStateConverter.ConvertStateToConsentForEdoStatus(stateCode);

		public async Task<ContactList> GetContactListUpdatesAsync(IUnitOfWork uow, int organizationId, DateTime dateLastRequest, ContactStateCode? status = null)
		{
			var key = await GetSettingsAndLogin(uow, organizationId);

			var uri = status is null
					? $"{_edoSettings.TaxcomGetContactListUpdatesUri}?DATE={dateLastRequest}"
					: $"{_edoSettings.TaxcomGetContactListUpdatesUri}?DATE={dateLastRequest}&STATUS={status}";

			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Add("Assistant-Key", key);

			var response = await _httpClient.SendAsync(request);

			if(response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsStreamAsync();

				return ContactListSerializer.DeserializeContactList(responseBody);
			}

			_edoLogger.LogError(response);

			return null;
		}
		
		public async Task<ContactListItem> GetLastChangeOnDate(
			IUnitOfWork uow,
			int organizationId,
			DateTime dateLastRequest,
			string inn,
			string kpp,
			ContactStateCode? status = null)
		{
			var items = new List<ContactListItem>();
			ContactList contactList;
			var date = dateLastRequest;

			do
			{
				contactList = await GetContactListUpdatesAsync(uow, organizationId, date, status);

				if(contactList.Contacts != null && contactList.Contacts.LastOrDefault() is ContactListItem item)
				{
					date = item.State.Changed;
					items.AddRange(contactList.Contacts);
				}

			} while(contactList.Contacts != null && contactList.Contacts.Length >= 100);

			return items
				.Where(x => x.Inn == inn
					&& (string.IsNullOrWhiteSpace(x.Kpp) || x.Kpp == kpp))
				.OrderByDescending(x => x.State.Changed)
				.FirstOrDefault();
		}
	}
}

