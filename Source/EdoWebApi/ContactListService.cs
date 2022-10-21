using EdoWebApi.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TISystems.TTC.CRM.BE.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EdoWebApi
{
	public class ContactListService : IContactListService
	{
		private readonly ILogger<ContactListService> _logger;
		private readonly HttpClient _httpClient;
		private readonly IConfigurationSection _contactListServiceSection;
		private readonly IConfigurationSection _trueApiSection;

		public ContactListService(
			ILogger<ContactListService> logger,
			HttpClient client,
			IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_contactListServiceSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("ContactListService");
			_trueApiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("TrueApi");
		}

		public async Task<ContactList> CheckContragentAsync(byte[] contacts, string assistantKey)
		{
			var content = PrepareContactsContent(contacts, assistantKey);
			var response = await _httpClient.PostAsync(_contactListServiceSection.GetValue<string>("CheckContragent"), content);

			if(response.IsSuccessStatusCode)
			{
				return ContactListSerializer.DeserializeContactList(await response.Content.ReadAsStreamAsync());
			}
			else
			{
				var statusCode = response.StatusCode;
				var reason = response.ReasonPhrase;
				_logger.LogError($"Http code {statusCode}, причина {reason}");
				return null;
			}
		}

		public async Task SendContactsAsync(byte[] contacts, string assistantKey)
		{
			var content = PrepareContactsContent(contacts, assistantKey);
			await _httpClient.PostAsync(_contactListServiceSection.GetValue<string>("SendContacts"), content);
		}

		public async Task<ContactList> GetContactListUpdatesAsync(DateTime dateLastRequest, string assistantKey, ContactStateCode? status)
		{
			var uri = status is null
				? $"{_contactListServiceSection.GetValue<string>("GetContactListUpdates")}?DATE={dateLastRequest}"
				: $"{_contactListServiceSection.GetValue<string>("GetContactListUpdates")}?DATE={dateLastRequest}&STATUS={status}";

			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Add("Assistant-Key", assistantKey);

			var response = await _httpClient.SendAsync(request);

			if(response.IsSuccessStatusCode)
			{
				return ContactListSerializer.DeserializeContactList(await response.Content.ReadAsStreamAsync());
			}
			else
			{
				var statusCode = response.StatusCode;
				var reason = response.ReasonPhrase;
				_logger.LogError($"Http code {statusCode}, причина {reason}");
				return null;
			}
		}

		public async Task<bool> GetHasCounterpartyInTrueApi(string inn)//7729076804
		{
			var uri =  $"{_trueApiSection.GetValue<string>("BaseAddress")}{_trueApiSection.GetValue<string>("CheckCounterpartyByInnEndpointURI")}?inns={inn}";
			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			var response = await _httpClient.SendAsync(request);

			if(response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				var registrationResult = JsonSerializer.Deserialize<List<TrueApiRegistrationDto>>(responseBody);
				return registrationResult != null && registrationResult.FirstOrDefault()!.IsRegistered;
			}

			var statusCode = response.StatusCode;
			var reason = response.ReasonPhrase;
			_logger.LogError($"Http code {statusCode}, причина {reason}");
			return false;
		}

		private ByteArrayContent PrepareContactsContent(byte[] contacts, string assistantKey)
		{
			var content = new ByteArrayContent(contacts);
			content.Headers.Add("Assistant-Key", assistantKey);
			content.Headers.ContentLength = contacts.Length;
			return content;
		}
	}
}

