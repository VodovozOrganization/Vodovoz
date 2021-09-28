using Bitrix.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Bitrix
{
	public class BitrixClient : IBitrixClient
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private static readonly HttpClient _httpClient = new HttpClient();

		static BitrixClient()
		{
			var jsonHeader = new MediaTypeWithQualityHeaderValue("application/json");

			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(jsonHeader);
		}

		private readonly string _token;
		private readonly string _userId;

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

			this._userId = userId;
			this._token = token;
		}

		#region real

		public Contact GetContact(uint id)
		{
			string requestUri = $"{Constants.ApiUrl}/rest/{_userId}/{_token}/crm.contact.get.json?id={id}";

			ContactResponse response = Get<ContactResponse>(requestUri);

			if(response == null)
			{
				return null;
			}

			return response.Result;
		}

		public Company GetCompany(uint id)
		{
			string requestUri = $"{Constants.ApiUrl}/rest/{_userId}/{_token}/crm.company.get.json?id={id}";

			CompanyResponse response = Get<CompanyResponse>(requestUri);

			if(response == null)
			{
				return null;
			}

			return response.Result;
		}

		public Product GetProduct(uint id)
		{
			string requestUri = $"{Constants.ApiUrl}/rest/{_userId}/{_token}/crm.product.get.json?id={id}";

			ProductResponse response = Get<ProductResponse>(requestUri);

			if(response == null)
			{
				return null;
			}

			return response.Result;
		}

		public IList<DealProductItem> GetProductsForDeal(uint dealId)
		{
			string requestUri = $"{Constants.ApiUrl}/rest/{_userId}/{_token}/crm.deal.productrows.get.json?id={dealId}";

			DealProductItemResponse response = Get<DealProductItemResponse>(requestUri);

			if(response == null || response.Result == null)
			{
				return new List<DealProductItem>();
			}

			return response.Result;
		}

		public IList<Deal> GetDeals(DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			List<Deal> deals = new List<Deal>();

			string dateFrom = dateTimeFrom.ToString("dd.MM.yyyy HH:mm:ss");
			string dateTo = dateTimeTo.ToString("dd.MM.yyyy HH:mm:ss");

			int next = 0;
			bool hasNext = true;
			do
			{
				string requestUri = $"{Constants.ApiUrl}/rest/{_userId}/{_token}/crm.deal.list.json?" +
					$"FILTER[>DATE_CREATE]={dateFrom}&" +
					$"FILTER[<DATE_CREATE]={dateTo}&" +
					//Включаем сделки только со временем доставки
					$"FILTER[>{UserFieldNames.DealDeliverySchedule}]=0&" +
					//Включаем сделки только в статусе Завести в ДВ
					$"FILTER[STAGE_ID]={Constants.CreateInDVDealStageId}" +
					//Добавляем в выгрузку все пользовательские поля
					$"&select[]=*&select[]=UF_*" +
					$"&start={next}";


				DealsResponse response = Get<DealsResponse>(requestUri);

				if(response == null || response.Result == null)
				{
					continue;
				}

				deals.AddRange(response.Result);

				_logger.Info($"Загружено сделок {deals.Count}/{response.Total}");
				hasNext = response.Next.HasValue;
				next = response.Next.HasValue ? response.Next.Value : 0;
			} while(hasNext);

			return deals;
		}

		private T Get<T>(string requestUri) where T : class
		{
			T response = null;
			try
			{
				var requestTask = _httpClient.GetStringAsync(requestUri);
				requestTask.Wait();
				var responseString = requestTask.Result;

				response = JsonConvert.DeserializeObject<T>(responseString);
			}
			catch(HttpRequestException ex)
			{
				_logger.Error(ex, "Не удалось получить ответ по запросу");
				throw;
			}
			catch(JsonException ex)
			{
				_logger.Error(ex, "Не удалось распарсить одну из сделок");
				throw;
			}

			return response;
		}

		#endregion

		#region Отправка статуса

		public bool SetStatusToDeal(DealStatus status, uint dealId)
		{
			//Вадим: Тут когда-то была эта задержка, не уверен что она сейчас нужна, необходимо проверить.
			//Задержка в 10сек, это необходимо из-за битрикса, он не успевает обработать какие-то скрипты
			//Thread.Sleep(10000);

			string stageId;

			switch(status)
			{
				case DealStatus.ToCreate:
					stageId = Constants.CreateInDVDealStageId;
					break;
				case DealStatus.InProgress:
					stageId = Constants.InProgressDealStageId;
					break;
				case DealStatus.Error:
					stageId = Constants.DVErrorDealStageId;
					break;
				case DealStatus.Success:
					stageId = Constants.SuccessDealStageId;
					break;
				case DealStatus.Fail:
					stageId = Constants.FailDealStageId;
					break;
				default:
					throw new InvalidOperationException("Неизвестный статус сделки");
			}

			string requestUri = $"{Constants.ApiUrl}/rest/{_userId}/{_token}/crm.deal.update.json?id={dealId}&FIELDS[STAGE_ID]={stageId}";

			var response = Get<ChangeStatusResult>(requestUri);

			return response.Result;
		}

		#endregion
	}
}