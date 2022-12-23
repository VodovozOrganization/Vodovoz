﻿using EdoService.Dto;
using NLog;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EdoService.Converters;
using TISystems.TTC.CRM.BE.Serialization;
using Vodovoz.Domain.Client;
using Vodovoz.Services;
using System.Text.RegularExpressions;

namespace EdoService.Services
{
	public class ContactListService : IContactListService
	{
		private readonly IAuthorizationService _authorizationService;
		private readonly IEdoSettings _edoSettings;
		private readonly IContactStateConverter _contactStateConverter;
		private static HttpClient _httpClient;
		private readonly IEdoLogger _edoLogger;

		public ContactListService(
			IAuthorizationService authorizationService,
			IEdoSettings edoSettings,
			IContactStateConverter contactStateConverter)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_contactStateConverter = contactStateConverter ?? throw new ArgumentNullException(nameof(contactStateConverter));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

			var logger = LogManager.GetCurrentClassLogger();
			_edoLogger = new EdoLogger(logger);

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

		public async Task<string> Login() => await _authorizationService.Login();

		public async Task<ContactList> CheckContragentAsync(string inn, string kpp)
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

		public async Task<ResultDto> SendContactsAsync(string inn, string kpp, string email, string edxClientId)
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
						Comment = "Компания Весёлый водовоз приглашает Вас к электронному обмену по типу продукции \"Питьевая вода.\""
					}
				}
			};

			return await SendContactsAsync(invitationsList);
		}

		public async Task<ResultDto> SendContactsForManualInvitationAsync(string inn, string kpp, string organizationName,
			string operatorId, string email, string scanFileName, byte[] scanFile)
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
						Comment = $"Компания {organizationName} приглашает Вас к электронному обмену по типу продукции \"Питьевая вода.\""
					}
				}
			};

			return await SendContactsAsync(invitationsList);
		}

		public async Task<ResultDto> SendContactsAsync(ContactList invitationsList)
		{
			var key = await Login();

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

		public async Task<ContactList> GetContactListUpdatesAsync(DateTime dateLastRequest, ContactStateCode? status = null)
		{
			var key = await Login();

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
	}
}

