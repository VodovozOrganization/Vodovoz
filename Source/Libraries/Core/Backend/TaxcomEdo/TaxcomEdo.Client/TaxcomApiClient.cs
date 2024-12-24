using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Edo.Transport.Messages.Dto;
using TaxcomEdo.Client.Configs;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdo.Client
{
	public partial class TaxcomApiClient : ITaxcomApiClient
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly TaxcomApiOptions _taxcomApiOptions;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		// Т.к. фабрика сама управляет созданными клиентами, то ее нужно регистрировать, как Singleton
		public TaxcomApiClient(
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
			await SendDocument(_taxcomApiOptions.SendUpdEndpoint, data);
		}
		
		public async Task<bool> SendDataForCreateUpdByEdo(UniversalTransferDocumentInfo data, CancellationToken cancellationToken = default)
		{
			return await SendDocument(_taxcomApiOptions.SendUpdEndpoint, data);
		}
		
		public async Task SendDataForCreateBillByEdo(InfoForCreatingEdoBill data, CancellationToken cancellationToken = default)
		{
			await SendDocument(_taxcomApiOptions.SendBillEndpoint, data);
		}
		
		public async Task SendDataForCreateBillWithoutShipmentForDebtByEdo(
			InfoForCreatingBillWithoutShipmentForDebtEdo data, CancellationToken cancellationToken = default)
		{
			await SendDocument(_taxcomApiOptions.SendBillWithoutShipmentForDebtEndpoint, data);
		}
		
		public async Task SendDataForCreateBillWithoutShipmentForPaymentByEdo(
			InfoForCreatingBillWithoutShipmentForPaymentEdo data, CancellationToken cancellationToken = default)
		{
			await SendDocument(_taxcomApiOptions.SendBillWithoutShipmentForPaymentEndpoint, data);
		}
		
		public async Task SendDataForCreateBillWithoutShipmentForAdvancePaymentByEdo(
			InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo data, CancellationToken cancellationToken = default)
		{
			await SendDocument(_taxcomApiOptions.SendBillWithoutShipmentForAdvancePaymentEndpoint, data);
		}

		public async Task<EdoContactList> GetContactListUpdates(
			DateTime? lastCheckContactsUpdates,
			EdoContactStateCode? contactState,
			CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(lastCheckContactsUpdates, nameof(lastCheckContactsUpdates))
				.AddParameter(contactState, nameof(contactState))
				.ToString();

			return await CreateClient().GetFromJsonAsync<EdoContactList>(
				_taxcomApiOptions.GetContactListUpdatesEndPoint + query, cancellationToken);
		}

		public async Task<bool> AcceptContact(string edxClientId, CancellationToken cancellationToken = default)
		{
			var result =
				await CreateClient().PostAsJsonAsync(_taxcomApiOptions.AcceptContactEndPoint, edxClientId, cancellationToken);

			return result.IsSuccessStatusCode;
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

		public async Task<EdoDocFlowUpdates> GetDocFlowsUpdates(
			GetDocFlowsUpdatesParameters docFlowsUpdatesParameters, CancellationToken cancellationToken = default)
		{
			using(var request = new HttpRequestMessage(HttpMethod.Get, _taxcomApiOptions.GetDocFlowsUpdatesEndPoint))
			{
				request.Content = JsonContent.Create(docFlowsUpdatesParameters);
				var client = CreateClient();

				using(var response = await client.SendAsync(request, cancellationToken))
				{
					if(!response.IsSuccessStatusCode)
					{
						return new EdoDocFlowUpdates();
					}

					using(var responseStream = await response.Content.ReadAsStreamAsync())
					{
						return await JsonSerializer.DeserializeAsync<EdoDocFlowUpdates>(
							responseStream, _jsonSerializerOptions, cancellationToken);
					}
				}
			}
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

		public async Task<bool> AcceptIngoingDocflow(Guid? docflowId, CancellationToken cancellationToken = default)
		{
			if(!docflowId.HasValue)
			{
				return false;
			}
			
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docflowId, nameof(docflowId))
				.ToString();
			
			var result = await CreateClient()
				.GetAsync(_taxcomApiOptions.AcceptIngoingDocflowEndpoint + query, cancellationToken);
			
			return result.IsSuccessStatusCode;
		}

		private async Task<bool> SendDocument<T>(string endPoint, T data)
		{
			var result = await CreateClient().PostAsJsonAsync(endPoint, data);
			return result.IsSuccessStatusCode;
		}

		private HttpClient CreateClient()
		{
			var client = _httpClientFactory.CreateClient(nameof(TaxcomApiClient));
			client.BaseAddress = new Uri(_taxcomApiOptions.BaseAddress);
			
			return client;
		}
	}
}
