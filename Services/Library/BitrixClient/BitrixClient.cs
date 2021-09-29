using Bitrix.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Bitrix
{
	public class BitrixClient : IBitrixClient
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly HttpClient _httpClient;

		public BitrixClient(string userId, string token)
		{
			if(string.IsNullOrWhiteSpace(userId))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
			}

			if(string.IsNullOrWhiteSpace(token))
			{
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
			}

			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri($"{Constants.ApiUrl}/rest/{userId}/{token}/")
			};

			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		#region real

		public async Task<Contact> GetContact(uint id)
		{
			string requestUri = $"crm.contact.get.json?id={id}";

			using HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
			var contactResponse = await response.Content.ReadAsAsync<ContactResponse>();

			if(contactResponse == null)
			{
				return null;
			}

			return contactResponse.Result;
		}

		public async Task<Company> GetCompany(uint id)
		{
			string requestUri = $"crm.company.get.json?id={id}";

			using HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
			var companyResponse = await response.Content.ReadAsAsync<CompanyResponse>();

			if(companyResponse == null)
			{
				return null;
			}

			return companyResponse.Result;
		}

		public async Task<Product> GetProduct(uint id)
		{
			string requestUri = $"crm.product.get.json?id={id}";

			using HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
			var productResponse = await response.Content.ReadAsAsync<ProductResponse>();

			if(productResponse == null)
			{
				return null;
			}

			return productResponse.Result;
		}

		public async Task<IList<DealProductItem>> GetProductsForDeal(uint dealId)
		{
			string requestUri = $"crm.deal.productrows.get.json?id={dealId}";

			using HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
			var dealProductItemResponse = await response.Content.ReadAsAsync<DealProductItemResponse>();

			if(dealProductItemResponse == null || dealProductItemResponse.Result == null)
			{
				return new List<DealProductItem>();
			}

			return dealProductItemResponse.Result;
		}

		public async Task<IList<Deal>> GetDeals(DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			List<Deal> deals = new List<Deal>();

			int next = 0;
			bool hasNext = true;
			do
			{
				string query = "crm.deal.list.json?";

				using(var content = new FormUrlEncodedContent(
					new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>("FILTER[>DATE_CREATE]", $"{dateTimeFrom:dd.MM.yyyy HH:mm:ss}"),
						new KeyValuePair<string, string>("FILTER[<DATE_CREATE]", $"{dateTimeTo:dd.MM.yyyy HH:mm: ss}"),
						new KeyValuePair<string, string>($"FILTER[>{UserFieldNames.DealDeliverySchedule}]", "0"),
						new KeyValuePair<string, string>($"FILTER[STAGE_ID]={Constants.CreateInDVDealStageId}", Constants.CreateInDVDealStageId),
						new KeyValuePair<string, string>("select[]", "*"),
						new KeyValuePair<string, string>("select[]", "UF_*"),
						new KeyValuePair<string, string>("start", $"{next}"),
					}))
				{
					query += await content.ReadAsStringAsync();
				}

				using HttpResponseMessage response = await _httpClient.GetAsync(query);
				var dealsResponse = await response.Content.ReadAsAsync<DealsResponse>();

				if(dealsResponse == null || dealsResponse.Result == null)
				{
					continue;
				}

				deals.AddRange(dealsResponse.Result);

				_logger.Info($"Загружено сделок {deals.Count}/{dealsResponse.Total}");
				hasNext = dealsResponse.Next.HasValue;
				next = dealsResponse.Next ?? 0;
			}
			while(hasNext);

			return deals;
		}

		#endregion

		#region Отправка статуса

		public async Task<bool> SetStatusToDeal(DealStatus status, uint dealId)
		{
			string stageId = status switch
			{
				DealStatus.ToCreate => Constants.CreateInDVDealStageId,
				DealStatus.InProgress => Constants.InProgressDealStageId,
				DealStatus.Error => Constants.DVErrorDealStageId,
				DealStatus.Success => Constants.SuccessDealStageId,
				DealStatus.Fail => Constants.FailDealStageId,
				_ => throw new InvalidOperationException("Неизвестный статус сделки"),
			};
			string requestUri = $"crm.deal.update.json?id={dealId}&FIELDS[STAGE_ID]={stageId}";

			using HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
			var changeStatusResponse = await response.Content.ReadAsAsync<ChangeStatusResult>();

			return changeStatusResponse.Result;
		}

		#endregion
	}
}
