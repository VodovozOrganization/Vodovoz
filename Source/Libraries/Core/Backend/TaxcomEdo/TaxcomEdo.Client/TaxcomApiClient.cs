using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Client.Configs;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdo.Client
{
	public class TaxcomApiClient : ITaxcomApiClient
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly TaxcomApiOptions _taxcomApiOptions;

		// Т.к. фабрика сама управляет созданными клиентами, то ее нужно регистрировать, как Singleton
		public TaxcomApiClient(
			IHttpClientFactory httpClientFactory,
			TaxcomApiOptions taxcomApiOptions)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_taxcomApiOptions = taxcomApiOptions ?? throw new ArgumentNullException(nameof(taxcomApiOptions));
		}
		
		public async Task SendDataForCreateUpdByEdo(InfoForCreatingEdoUpd data, CancellationToken cancellationToken = default)
		{
			await SendDocument(_taxcomApiOptions.SendUpdEndpoint, data);
		}
		
		public async Task SendDataForCreateBillByEdo(InfoForCreatingEdoBill data, CancellationToken cancellationToken = default)
		{
			await SendDocument(_taxcomApiOptions.SendBillEndpoint, data);
		}
		
		public async Task SendDataForCreateBillWithoutShipmentByEdo(
			InfoForCreatingBillWithoutShipmentEdo data, CancellationToken cancellationToken = default)
		{
			await SendDocument(_taxcomApiOptions.SendBillsWithoutShipmentEndpoint, data);
		}

		public async Task<EdoContactList> GetContactListUpdates(
			DateTime? lastCheckContactsUpdates,
			EdoContactStateCode? contactState,
			CancellationToken cancellationToken = default)
		{
			return await CreateClient().GetFromJsonAsync<EdoContactList>(
				GetGetContactListUpdatesRequestUri(lastCheckContactsUpdates, contactState), cancellationToken);
		}

		public async Task AcceptContact(string edxClientId, CancellationToken cancellationToken = default)
		{
			await CreateClient().PostAsJsonAsync(_taxcomApiOptions.AcceptContactEndPoint, edxClientId, cancellationToken);
		}

		public async Task<IEnumerable<byte>> GetDocFlowRawData(string docFlowId, CancellationToken cancellationToken = default)
		{
			var response = await CreateClient().PostAsJsonAsync(_taxcomApiOptions.GetDocFlowRawDataEndPoint, docFlowId, cancellationToken);
			
			if(!response.IsSuccessStatusCode)
			{
				return Enumerable.Empty<byte>();
			}
			
			using(var responseStream = await response.Content.ReadAsStreamAsync())
			{
				return await JsonSerializer.DeserializeAsync<IEnumerable<byte>>(responseStream);
			}
		}

		public async Task<EdoDocFlowUpdates> GetDocFlowsUpdates(
			GetDocFlowsUpdatesParameters docFlowsUpdatesParameters, CancellationToken cancellationToken = default)
		{
			var response = await CreateClient()
				.PostAsJsonAsync(_taxcomApiOptions.GetDocFlowsUpdatesEndPoint, docFlowsUpdatesParameters, cancellationToken);

			if(!response.IsSuccessStatusCode)
			{
				return new EdoDocFlowUpdates();
			}

			using(var responseStream = await response.Content.ReadAsStreamAsync())
			{
				return await JsonSerializer.DeserializeAsync<EdoDocFlowUpdates>(responseStream);
			}
		}

		private async Task SendDocument<T>(string endPoint, T data)
		{
			await CreateClient().PostAsJsonAsync(endPoint, data);
		}

		private HttpClient CreateClient(string clientName = null)
		{
			var client = _httpClientFactory.CreateClient(clientName);
			client.BaseAddress = new Uri(_taxcomApiOptions.BaseAddress);
			
			return client;
		}

		private string GetGetContactListUpdatesRequestUri(DateTime? lastCheckContactsUpdates, EdoContactStateCode? contactState)
		{
			var requestUriBuilder = new StringBuilder();
			requestUriBuilder.Append(_taxcomApiOptions.GetContactListUpdatesEndPoint);
			var hasFirstParameter = false;

			if(lastCheckContactsUpdates.HasValue)
			{
				requestUriBuilder
					.Append("?")
					.Append(nameof(lastCheckContactsUpdates))
					.Append("=")
					.Append(lastCheckContactsUpdates.ToString());

				hasFirstParameter = true;
			}

			if(contactState.HasValue)
			{
				if(hasFirstParameter)
				{
					requestUriBuilder.Append("&");
				}
				
				requestUriBuilder
					.Append("?")
					.Append(nameof(contactState))
					.Append("=")
					.Append(contactState.ToString());
			}

			return requestUriBuilder.ToString();
		}
	}
}
