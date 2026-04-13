using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Extensions;
using TaxcomEdo.Contracts.Responses;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library.Services
{
	public class EdoContactService : EdoLoginService, IEdoContactService
	{
		private readonly HttpClient _httpClient;

		public EdoContactService(
			HttpClient httpClient,
			IEdoAuthorizationService edoAuthorizationService
			) : base(edoAuthorizationService)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}
		
		public async Task<TaxcomResponse<EdoContactList>> CheckCounterpartyAsync(
			EdoContactList contactList, string login, string password, CancellationToken cancellationToken = default)
		{
			var key = await LoginAsync(login, password, cancellationToken: cancellationToken);
			return await CheckCounterpartyAsync(key, contactList.ToXmlString(), cancellationToken);
		}
		
		public async Task<TaxcomResponse<EdoContactList>> CheckCounterpartyAsync(
			EdoContactList contactList, byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var key = await CertificateLoginAsync(certificateData, cancellationToken);
			return await CheckCounterpartyAsync(key, contactList.ToXmlString(), cancellationToken);
		}

		public async Task<TaxcomResponse> SendContactsAsync(
			EdoContactList contactsList, string login, string password, CancellationToken cancellationToken = default)
		{
			var key = await LoginAsync(login, password, cancellationToken: cancellationToken);
			return await SendContactsAsync(key, contactsList.ToXmlString(), cancellationToken);
		}

		public async Task<TaxcomResponse> SendContactsAsync(
			EdoContactList contactsList, byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var key = await CertificateLoginAsync(certificateData, cancellationToken);
			return await SendContactsAsync(key, contactsList.ToXmlString(), cancellationToken);
		}

		public async Task<TaxcomResponse> AcceptContactAsync(
			string clientEdoAccountId, string login, string password, CancellationToken cancellationToken = default)
		{
			var key = await LoginAsync(login, password, cancellationToken: cancellationToken);
			return await AcceptContactAsync(key, clientEdoAccountId, cancellationToken);
		}
		
		public async Task<TaxcomResponse> AcceptContactAsync(
			string clientEdoAccountId, byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var key = await CertificateLoginAsync(certificateData, cancellationToken);
			return await AcceptContactAsync(key, clientEdoAccountId, cancellationToken);
		}

		public async Task<TaxcomResponse> RejectContactAsync(
			string clientEdoAccountId, string login, string password, string comment = null, CancellationToken cancellationToken = default)
		{
			var key = await LoginAsync(login, password, cancellationToken: cancellationToken);
			return await RejectContactAsync(key, clientEdoAccountId, comment, cancellationToken);
		}

		public async Task<TaxcomResponse> RejectContactAsync(
			string clientEdoAccountId, byte[] certificateData, string comment = null, CancellationToken cancellationToken = default)
		{
			var key = await CertificateLoginAsync(certificateData, cancellationToken);
			return await RejectContactAsync(key, clientEdoAccountId, comment, cancellationToken);
		}

		public async Task<TaxcomResponse<EdoContactList>> GetContactListUpdatesAsync(
			DateTime dateTime, EdoContactStateCode? stateCode, string login, string password, CancellationToken cancellationToken = default)
		{
			var key = await LoginAsync(login, password, cancellationToken: cancellationToken);
			var response = await GetContactListUpdatesAsync(dateTime, stateCode, key, cancellationToken);
			
			return response;
		}
		
		public async Task<TaxcomResponse<EdoContactList>> GetContactListUpdatesAsync(
			DateTime dateTime, EdoContactStateCode? stateCode, byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var key = await CertificateLoginAsync(certificateData, cancellationToken);
			var response = await GetContactListUpdatesAsync(dateTime, stateCode, key, cancellationToken);

			return response;
		}

		public async Task<TaxcomResponse<EdoContactList>> GetContactsAsync(
			string login, string password, CancellationToken cancellationToken = default)
		{
			var key = await LoginAsync(login, password, cancellationToken: cancellationToken);
			return await GetContactsAsync(key, cancellationToken);
		}

		public async Task<TaxcomResponse<EdoContactList>> GetContactsAsync(byte[] certificateData, CancellationToken cancellationToken = default)
		{
			var key = await CertificateLoginAsync(certificateData, cancellationToken);
			return await GetContactsAsync(key, cancellationToken);
		}
		
		private async Task<TaxcomResponse<EdoContactList>> GetContactListUpdatesAsync(
			DateTime dateTime, EdoContactStateCode? stateCode, string assistantKey, CancellationToken cancellationToken)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(dateTime, "date");

			if(stateCode.HasValue)
			{
				query.AddParameter(stateCode.Value, "status");
			}

			var message = PrepareGetHttpRequestMessage(assistantKey, ExternalApiConstants.GetContactListUpdatesUri + query.ToString());
			var response = await _httpClient.SendAsync(message, cancellationToken);
			
			return await response.ToTaxcomResponseAsync<EdoContactList>(cancellationToken);
		}
		
		private async Task<TaxcomResponse> SendContactsAsync(string assistantKey, string contactList, CancellationToken cancellationToken)
		{
			var content = PrepareByteArrayContent(Encoding.UTF8.GetBytes(contactList), assistantKey);
			var response = await _httpClient.PostAsync(ExternalApiConstants.SendContactsUri, content, cancellationToken);
			
			return response.ToTaxcomResponse();
		}
		
		private async Task<TaxcomResponse<EdoContactList>> CheckCounterpartyAsync(
			string assistantKey, string contactList, CancellationToken cancellationToken)
		{
			var content = PrepareByteArrayContent(Encoding.UTF8.GetBytes(contactList), assistantKey);
			var response = await _httpClient.PostAsync(ExternalApiConstants.CheckContragentUri, content, cancellationToken);

			return await response.ToTaxcomResponseAsync<EdoContactList>(cancellationToken);
		}
		
		private async Task<TaxcomResponse> AcceptContactAsync(string assistantKey, string clientEdoAccountId, CancellationToken cancellationToken)
		{
			var query = HttpQueryBuilder.Create()
				.AddParameter(clientEdoAccountId, "id")
				.ToString();
			
			var message = PrepareGetHttpRequestMessage(assistantKey, ExternalApiConstants.AcceptContactUri + query);
			var response = await _httpClient.SendAsync(message, cancellationToken);
			
			return response.ToTaxcomResponse();
		}
		
		private async Task<TaxcomResponse> RejectContactAsync(
			string assistantKey, string clientEdoAccountId, string comment, CancellationToken cancellationToken)
		{
			//TODO проверить работу отклонения контакта
			var query = HttpQueryBuilder.Create()
				.AddParameter(clientEdoAccountId, "id");

			if(!string.IsNullOrWhiteSpace(comment))
			{
				query.AddParameter("comment", comment);
			}

			var message = PrepareGetHttpRequestMessage(assistantKey, ExternalApiConstants.RejectContactUri + query.ToString());
			var response = await _httpClient.SendAsync(message, cancellationToken);
			
			return response.ToTaxcomResponse();
		}

		private async Task<TaxcomResponse<EdoContactList>> GetContactsAsync(string assistantKey, CancellationToken cancellationToken)
		{
			var message = PrepareGetHttpRequestMessage(assistantKey, ExternalApiConstants.GetContactsUri);
			var response = await _httpClient.SendAsync(message, cancellationToken);
			
			return await response.ToTaxcomResponseAsync<EdoContactList>(cancellationToken);
		}
	}
}
