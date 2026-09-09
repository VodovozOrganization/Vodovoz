using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Client.Configs;
using TaxcomEdo.Contracts.Contacts;
using TaxcomEdo.Contracts.Counterparties;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Results;
using VodovozInfrastructure.Endpoints;

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

		public async Task<Result> SendDataForCreateUpdByEdo(InfoForCreatingEdoUpd data, CancellationToken cancellationToken = default)
		{
			_taxcomApiOptions.SendBulkAccountingUpdEndpoint = "/api/CreateAndSendBulkAccountingUpd";
			return await SendDocument(_taxcomApiOptions.SendBulkAccountingUpdEndpoint, data, cancellationToken: cancellationToken);
		}

		public Task<Result> SendDataForCreateUpdByEdo(UniversalTransferDocumentInfo data, CancellationToken cancellationToken = default)
		{
			return SendDocument(_taxcomApiOptions.SendIndividualAccountingUpdEndpoint, data, cancellationToken: cancellationToken);
		}

		public async Task<Result> SendDataForCreateBillByEdo(InfoForCreatingEdoBill data, CancellationToken cancellationToken = default)
		{
			var ourEdxId = data.OrderInfoForEdo.ContractInfoForEdo.OrganizationInfoForEdo.TaxcomEdoAccountId;
			return await SendDocument(_taxcomApiOptions.SendBillEndpoint, data, ourEdxId, cancellationToken);
		}

		public async Task<Result> SendDataForCreateInformalOrderDocumentByEdo(InfoForCreatingEdoInformalOrderDocument data,
			CancellationToken cancellationToken = default)
		{
			return await SendDocument(_taxcomApiOptions.SendInformalOrderDocumentEndpoint, data, cancellationToken: cancellationToken);
		}

		public async Task<Result> SendDataForCreateBillWithoutShipmentForDebtByEdo(InfoForCreatingBillWithoutShipmentForDebtEdo data,
			CancellationToken cancellationToken = default)
		{
			var ourEdxId = data.OrderWithoutShipmentForDebtInfo.OrganizationInfoForEdo.TaxcomEdoAccountId;
			return await SendDocument(_taxcomApiOptions.SendBillWithoutShipmentForDebtEndpoint, data, ourEdxId, cancellationToken);
		}

		public async Task<Result> SendDataForCreateBillWithoutShipmentForPaymentByEdo(InfoForCreatingBillWithoutShipmentForPaymentEdo data,
			CancellationToken cancellationToken = default)
		{
			var ourEdxId = data.OrderWithoutShipmentForPaymentInfo.OrganizationInfoForEdo.TaxcomEdoAccountId;
			return await SendDocument(_taxcomApiOptions.SendBillWithoutShipmentForPaymentEndpoint, data, ourEdxId, cancellationToken);
		}

		public async Task<Result> SendDataForCreateBillWithoutShipmentForAdvancePaymentByEdo(
			InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo data, CancellationToken cancellationToken = default)
		{
			var ourEdxId = data.OrderWithoutShipmentForAdvancePaymentInfo.OrganizationInfoForEdo.TaxcomEdoAccountId;
			return await SendDocument(_taxcomApiOptions.SendBillWithoutShipmentForAdvancePaymentEndpoint, data, ourEdxId, cancellationToken);
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

		public async Task<Result<byte[]>> GetDocFlowRawData(string docFlowId, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docFlowId, nameof(docFlowId))
				.ToString();

			_taxcomApiOptions.GetDocFlowRawDataEndPoint = "/api/GetDocFlowRawData";

			var response = await CreateClient()
				.GetAsync(_taxcomApiOptions.GetDocFlowRawDataEndPoint + query, cancellationToken);

			if(response.IsSuccessStatusCode)
			{
				var documents = await response.Content
					.ReadFromJsonAsync<byte[]>(_jsonSerializerOptions, cancellationToken);

				return Result.Success(documents ?? Array.Empty<byte>());
			}

			var error = await response.ToTaxcomError(_jsonSerializerOptions, cancellationToken);

			return Result.Failure<byte[]>(error);
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

		public async Task<ContainerDescription> GetDocflowStatus(string docflowId, string ourEdoAccountId = null)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docflowId, nameof(docflowId))
				.ToString();

			var response = await CreateClient(ourEdoAccountId)
				.GetStringAsync(_taxcomApiOptions.GetDocflowStatusEndpoint + query);

			return response.DeserializeXmlString<ContainerDescription>();
		}

		private async Task<Result> SendDocument<T>(
			string endPoint, T data, string ourEdxId = null, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await CreateClient(ourEdxId).PostAsJsonAsync(endPoint, data, cancellationToken);

				if(response.IsSuccessStatusCode)
				{
					return Result.Success();
				}

				var error = await response.ToTaxcomError(_jsonSerializerOptions, cancellationToken);

				return Result.Failure(error);
			}
			catch(Exception ex)
			{
				return Result.Failure(new Error("TaxcomUnexpectedError", ex.Message));
			}
		}

		private HttpClient CreateClient(string ourEdoAccountId = null)
		{
			var client = _httpClientFactory.CreateClient(nameof(TaxcomApiClient));

			if(ourEdoAccountId is null)
			{
				if(string.IsNullOrEmpty(_taxcomApiOptions.MainEdoAccountBaseAddress))
				{
					throw new ConfigurationErrorsException($"В конфигурации не настроен основной адрес организации");
				}

				client.BaseAddress = new Uri(_taxcomApiOptions.MainEdoAccountBaseAddress);
			}
			else
			{
				var ourEdoAccount = _taxcomApiOptions.EdoAccountBaseAddresses.SingleOrDefault(x => x.EdoAccountId == ourEdoAccountId);

				if(ourEdoAccount is null)
				{
					throw new ConfigurationErrorsException($"В конфигурации не настроен адрес апи для организации с аккаунтом {ourEdoAccountId}");
				}

				client.BaseAddress = new Uri(ourEdoAccount.BaseAddress);
			}

			return client;
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

		public async Task<Result> AcceptOfferCancellation(string docFlowId, CancellationToken cancellationToken = default)
		{
			var query = HttpQueryBuilder
				.Create()
				.AddParameter(docFlowId, nameof(docFlowId))
				.ToString();

			var response = await CreateClient()
				.GetAsync("/api/AcceptOfferCancellation" + query, cancellationToken);

			if(response.IsSuccessStatusCode)
			{
				return Result.Success();
			}

			var error = await response.ToTaxcomError(_jsonSerializerOptions, cancellationToken);

			return Result.Failure(error);
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
	}
}
