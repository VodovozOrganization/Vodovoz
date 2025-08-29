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
		
		public async Task<string> CheckCounterpartyAsync(string inn, string kpp, string login, string password)
		{
			var key = await Login(login, password);
			var contactList = EdoContactList.CreateForCheckContragent(inn, kpp);
			
			return await CheckCounterpartyAsync(key, contactList.ToXmlString());
		}
		
		public async Task<string> CheckCounterpartyAsync(string inn, string kpp, byte[] certificateData)
		{
			var key = await CertificateLogin(certificateData);
			var contactList = EdoContactList.CreateForCheckContragent(inn, kpp);
			
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

		public async Task<bool> AcceptContactAsync(string edoAccountId)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(edoAccountId, "id")
				.ToString();
			
			var response = await _httpClient.GetAsync(_options.AcceptContactUri + query);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> RejectContactAsync(string edoAccountId)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(edoAccountId, "id")
				.ToString();
			
			var response = await _httpClient.GetAsync(_options.RejectContactUri + query);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> GetContactListUpdatesAsync(string edoAccountId)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(edoAccountId, "id")
				.ToString();
			
			var response = await _httpClient.GetAsync(_options.RejectContactUri + query);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> GetContactsAsync(string edoAccountId)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(edoAccountId, "id")
				.ToString();
			
			var response = await _httpClient.GetAsync(_options.RejectContactUri + query);
			return response.IsSuccessStatusCode;
		}
		
		private async Task<bool> SendContactsAsync(string key, string contactList)
		{
			var content = PrepareContactsContent(Encoding.UTF8.GetBytes(contactList), key);
			var response = await _httpClient.PostAsync(_options.SendContactsUri, content);
			
			return response.IsSuccessStatusCode;
		}
		
		private async Task<string> CheckCounterpartyAsync(string key, string contactList)
		{
			var content = PrepareContactsContent(Encoding.UTF8.GetBytes(contactList), key);
			var response = await _httpClient.PostAsync(_options.CheckContragentUri, content);
			
			return response.IsSuccessStatusCode;
		}
		
		private ByteArrayContent PrepareContactsContent(byte[] contacts, string assistantKey)
		{
			var content = new ByteArrayContent(contacts);
			content.Headers.Add("Assistant-Key", assistantKey);
			content.Headers.ContentLength = contacts.Length;
			return content;
		}
	}

	public interface IEdoContactService
	{
		Task<string> CheckCounterpartyAsync(string inn, string kpp, string login, string password);
		Task<string> CheckCounterpartyAsync(string inn, string kpp, byte[] certificateData);
		Task<bool> SendContactsAsync(string contactList, string login, string password);
		Task<bool> SendContactsAsync(string contactList, byte[] certificateData);
		Task<bool> AcceptContactAsync(string clientEdoAccountId);
		Task<bool> RejectContactAsync(string edoAccountId);
	}
}
