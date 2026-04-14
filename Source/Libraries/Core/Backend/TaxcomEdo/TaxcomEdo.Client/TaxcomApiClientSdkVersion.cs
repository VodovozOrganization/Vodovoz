using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using TaxcomEdo.Client.Configs;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Extensions;
using TaxcomEdo.Contracts.Responses;
using TaxcomEdo.Contracts.Xml.Container;

namespace TaxcomEdo.Client
{
	public partial class TaxcomApiClientSdkVersion : ITaxcomApiClientSdkVersion
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly TaxcomApiOptions _taxcomApiOptions;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		// Т.к. фабрика сама управляет созданными клиентами, то ее нужно регистрировать, как Singleton
		public TaxcomApiClientSdkVersion(
			IHttpClientFactory httpClientFactory,
			TaxcomApiOptions taxcomApiOptions,
			JsonSerializerOptions jsonSerializerOptions)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_taxcomApiOptions = taxcomApiOptions ?? throw new ArgumentNullException(nameof(taxcomApiOptions));
			_jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
		}
		
		public async Task SendDataForCreateUpdByEdo(InfoForCreatingEdoUpd data, CancellationToken cancellationToken = default)
		{
			await SendAsJson(_taxcomApiOptions.SendBulkAccountingUpdEndpoint, data, cancellationToken);
		}
		
		public async Task<TaxcomResponse> SendDataForCreateUpdByEdo(UniversalTransferDocumentInfo data, CancellationToken cancellationToken = default)
		{
			return await SendAsJson(_taxcomApiOptions.SendIndividualAccountingUpdEndpoint, data, cancellationToken);
		}
		
		public async Task<TaxcomResponse> SendDataForCreateBillByEdo(InfoForCreatingEdoBill data, CancellationToken cancellationToken = default)
		{
			return await SendAsJson(_taxcomApiOptions.SendBillEndpoint, data, cancellationToken);
		}
		
		public async Task<TaxcomResponse> SendDataForCreateBillWithoutShipmentForDebtByEdo(
			InfoForCreatingBillWithoutShipmentForDebtEdo data, CancellationToken cancellationToken = default)
		{
			return await SendAsJson(_taxcomApiOptions.SendBillWithoutShipmentForDebtEndpoint, data, cancellationToken);
		}
		
		public async Task<TaxcomResponse> SendDataForCreateBillWithoutShipmentForPaymentByEdo(
			InfoForCreatingBillWithoutShipmentForPaymentEdo data, CancellationToken cancellationToken = default)
		{
			return await SendAsJson(_taxcomApiOptions.SendBillWithoutShipmentForPaymentEndpoint, data, cancellationToken);
		}
		
		public async Task<TaxcomResponse> SendDataForCreateBillWithoutShipmentForAdvancePaymentByEdo(
			InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo data, CancellationToken cancellationToken = default)
		{
			return await SendAsJson(_taxcomApiOptions.SendBillWithoutShipmentForAdvancePaymentEndpoint, data, cancellationToken);
		}

		public async Task<TaxcomResponse<EdoContactList>> GetContactListUpdates(
			DateTime? lastCheckContactsUpdates,
			EdoContactStateCode? contactState,
			CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(lastCheckContactsUpdates, nameof(lastCheckContactsUpdates))
				.AddParameter(contactState, nameof(contactState))
				.ToString();
			
			var response = await CreateClient().GetFromJsonAsync<TaxcomResponse<EdoContactList>>(
				_taxcomApiOptions.GetContactListUpdatesEndPoint + query, cancellationToken);

			return response;
		}

		public async Task<TaxcomResponse> AcceptContact(string edxClientId, CancellationToken cancellationToken = default)
		{
			var result =
				await CreateClient().PostAsJsonAsync(_taxcomApiOptions.AcceptContactEndPoint, edxClientId, cancellationToken);

			return result.ToTaxcomResponse();
		}

		public async Task<TaxcomResponse<EdoContactList>> CheckCounterpartyAsync(
			string inn,
			string kpp,
			CancellationToken cancellationToken = default)
		{
			var contactList = EdoContactList.CreateForCheckCounterparty(inn, kpp);
			
			var response = await CreateClient()
				.PostAsJsonAsync("/api/CheckCounterparty", contactList, cancellationToken);
			var responseStream = await response.Content.ReadAsStreamAsync();
			
			return await JsonSerializer.DeserializeAsync<TaxcomResponse<EdoContactList>>(responseStream, _jsonSerializerOptions, cancellationToken);
		}
		
		public async Task<TaxcomResponse> SendContactsAsync(
			string inn, string kpp, string email, string edxClientId, string organization, CancellationToken cancellationToken = default)
		{
			var comment = $"Компания {organization} приглашает Вас к электронному обмену документами";
			var invitationsList = EdoContactList.Create(inn, kpp, email, edxClientId, comment);
			
			return await SendContactsAsync(invitationsList, cancellationToken);
		}
		
		public async Task<TaxcomResponse> SendContactsForManualInvitationAsync(
			string inn,
			string kpp,
			string organizationName,
			string operatorId,
			string email,
			string scanFileName,
			byte[] scanFile,
			CancellationToken cancellationToken = default)
		{
			var comment = $"Компания {organizationName} приглашает Вас к электронному обмену документами";
			
			var invitationsList = EdoContactList.Create(
				inn,
				kpp,
				organizationName,
				operatorId,
				email,
				scanFileName,
				Convert.ToBase64String(scanFile),
				comment);

			return await SendContactsAsync(invitationsList, cancellationToken);
		}
		
		public async Task<TaxcomResponse> SendContactsAsync(EdoContactList contactList, CancellationToken cancellationToken = default)
		{
			return await SendAsJson("/api/SendContacts", contactList, cancellationToken);
		}

		public async Task<IEnumerable<byte>> GetDocFlowRawData(string docFlowId, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docFlowId, nameof(docFlowId))
				.ToString();

			var response = await CreateClient()
				.GetByteArrayAsync(_taxcomApiOptions.GetDocFlowRawDataEndPoint + query);

			return response;
		}

		public async Task<TaxcomResponse<EdoDocFlowUpdates>> GetDocFlowsUpdates(
			GetDocFlowsUpdatesParameters docFlowsUpdatesParameters, CancellationToken cancellationToken = default)
		{
			var response = await CreateClient()
				.PostAsJsonAsync(_taxcomApiOptions.GetDocFlowsUpdatesEndPoint, docFlowsUpdatesParameters, cancellationToken);
			var responseStream = await response.Content.ReadAsStreamAsync();
			
			return await JsonSerializer
				.DeserializeAsync<TaxcomResponse<EdoDocFlowUpdates>>(responseStream, _jsonSerializerOptions, cancellationToken);
		}

		public async Task StartProcessAutoSendReceive(CancellationToken cancellationToken = default)
		{
			await CreateClient().GetAsync(_taxcomApiOptions.AutoSendReceiveEndpoint, cancellationToken);
		}

		public async Task SendOfferCancellation(string docFlowId, string reason, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docFlowId, nameof(docFlowId))
				.AddParameter(reason, nameof(reason))
				.ToString();
			
			await CreateClient()
				.GetAsync(_taxcomApiOptions.OfferCancellationEndpoint + query, cancellationToken);
		}

		public async Task<bool> AcceptIngoingDocflow(Guid? docflowId, string organization, CancellationToken cancellationToken = default)
		{
			if(!docflowId.HasValue)
			{
				return false;
			}
			
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docflowId, nameof(docflowId))
				.AddParameter(organization, nameof(organization))
				.ToString();
			
			var result = await CreateClient()
				.GetAsync(_taxcomApiOptions.AcceptIngoingDocflowEndpoint + query, cancellationToken);
			
			return result.IsSuccessStatusCode;
		}
		
		public async Task SendOfferCancellationRaw(string docFlowId, string comment, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docFlowId, nameof(docFlowId))
				.AddParameter(comment, nameof(comment))
				.ToString();

			await CreateClient()
				.GetAsync("/api/SendOfferCancellation" + query, cancellationToken);
		}

		public async Task AcceptOfferCancellation(string docFlowId, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docFlowId, nameof(docFlowId))
				.ToString();

			await CreateClient()
				.GetAsync("/api/AcceptOfferCancellation" + query, cancellationToken);
		}

		public async Task RejectOfferCancellation(string docFlowId, string comment, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docFlowId, nameof(docFlowId))
				.AddParameter(comment, nameof(comment))
				.ToString();

			await CreateClient()
				.GetAsync("/api/RejectOfferCancellation" + query, cancellationToken);
		}

		private async Task<TaxcomResponse> SendAsJson<T>(string endPoint, T data, CancellationToken cancellationToken = default)
		{
			var result = await CreateClient().PostAsJsonAsync(endPoint, data, cancellationToken);
			return result.ToTaxcomResponse();
		}
		
		private async Task<ContainerDescription> GetDocflowStatus(string docflowId)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docflowId, nameof(docflowId))
				.ToString();

			var response = await CreateClient()
				.GetStringAsync(_taxcomApiOptions.GetDocflowStatusEndpoint + query);

			return response.DeserializeXmlString<ContainerDescription>();
		}

		private HttpClient CreateClient()
		{
			var client = _httpClientFactory.CreateClient(nameof(TaxcomApiClientSdkVersion));
			client.BaseAddress = new Uri(_taxcomApiOptions.BaseAddress);
			
			return client;
		}
	}
}
