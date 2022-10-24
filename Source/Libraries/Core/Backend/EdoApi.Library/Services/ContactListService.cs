using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TISystems.TTC.CRM.BE.Serialization;

namespace EdoApi.Library.Services
{
	public class ContactListService : IContactListService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfigurationSection _contactListServiceSection;
		private readonly IEdoLogger _edoLogger;

		public ContactListService(
			ILogger<ContactListService> logger,
			HttpClient client,
			IConfiguration configuration)
		{
			_edoLogger = new EdoLogger<ContactListService>(logger ?? throw new ArgumentNullException(nameof(logger)));
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_contactListServiceSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("ContactListService");
		}

		public async Task<ContactList> CheckContragentAsync(byte[] contacts, string assistantKey)
		{
			var content = PrepareContactsContent(contacts, assistantKey);
			var response = await _httpClient.PostAsync(_contactListServiceSection.GetValue<string>("CheckContragent"), content);

			if(response.IsSuccessStatusCode)
			{
				return ContactListSerializer.DeserializeContactList(await response.Content.ReadAsStreamAsync());
			}

			_edoLogger.LogError(response);

			return null;
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

			_edoLogger.LogError(response);

			return null;
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

