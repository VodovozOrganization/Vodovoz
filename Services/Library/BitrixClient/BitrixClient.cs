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
				string requestUri = $"crm.deal.list.json?" +
					$"FILTER[>DATE_CREATE]={dateTimeFrom:dd.MM.yyyy HH:mm:ss}&" +
					$"FILTER[<DATE_CREATE]={dateTimeTo:dd.MM.yyyy HH:mm:ss}&" +
					//Включаем сделки только со временем доставки
					$"FILTER[>{UserFieldNames.DealDeliverySchedule}]=0&" +
					//Включаем сделки только в статусе Завести в ДВ
					$"FILTER[STAGE_ID]={Constants.CreateInDVDealStageId}" +
					//Добавляем в выгрузку все пользовательские поля
					$"&select[]=*&select[]=UF_*" +
					$"&start={next}";

				using HttpResponseMessage response = await _httpClient.GetAsync(requestUri);
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
