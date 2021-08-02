using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BitrixApi.DTO;
using Newtonsoft.Json;

namespace BitrixApi.REST
{
	public class BitrixRestApi : IBitrixRestApi
    {
        static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly HttpClient client = new HttpClient();
        private string token;
        private string userId;
        private const string baseURL = "https://vodovoz.bitrix24.ru";
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);
        private const string createInDVStageId = "13";
        
        
        /// <param name="token">BitrixAPI токен из конфига</param>
        public BitrixRestApi(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            this.userId = userId;
            this.token = token;
        }
        
        //crm.deal.get
        public async Task<Deal> GetDealAsync( uint id )
        {
            AddJsonHeader();
            
            string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.deal.get.json?id={id}";
            var msg = client.GetStringAsync(requestUri);
            await semaphoreSlim.WaitAsync();
            DealResponse request = null;
            try{
                logger.Info("Ждем Deal");
                Thread.Sleep(20);
                request = JsonConvert.DeserializeObject<DealResponse>(await msg);
                logger.Info("Подождали Deal");
            }
            finally{
                semaphoreSlim.Release();
            }
           
            return request.Result;
        }
        
        //crm.contact.get
        public Contact GetContact( uint id )
        {
			throw new NotImplementedException();
			/*
            AddJsonHeader();
            string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.contact.get.json?id={id}";
            var msg = client.GetStringAsync(requestUri);
            await semaphoreSlim.WaitAsync();

            ContactRequest request = null;
            try{
                logger.Info("Ждем Contact");
                Thread.Sleep(1000);
                request = JsonConvert.DeserializeObject<ContactRequest>(await msg);
                logger.Info("Подождали Contact");
            }
            finally{
                semaphoreSlim.Release();
            }
            
            return request.Result;
			*/ 
        }
        
        //crm.company.get
        public Company GetCompany( uint id )
        {
			throw new NotImplementedException();
			/*
			AddJsonHeader();
            string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.company.get.json?id={id}";
            var msg = client.GetStringAsync(requestUri);
            await semaphoreSlim.WaitAsync();

            CompanyResponse request = null;
            try{
                logger.Info("Ждем Company");
                Thread.Sleep(1000);
                request = JsonConvert.DeserializeObject<CompanyResponse>(await msg);
                logger.Info("Подождали Company");
            }
            finally{
                semaphoreSlim.Release();
            }
            
            return request.Result;
			*/
        }

        //crm.product.get
        public async Task<Product> GetProduct( uint id )
        {
            AddJsonHeader();
            string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.product.get.json?id={id}";
            var productDataTask = client.GetStringAsync(requestUri);
            await semaphoreSlim.WaitAsync();

            ProductResponse request = null;
            try{
                logger.Info("Ждем Product");

                Thread.Sleep(1000);
                var productData = await productDataTask;
                request = JsonConvert.DeserializeObject<ProductResponse>(productData);
                logger.Info("Подождали Product");

            }
            finally{
                semaphoreSlim.Release();
            }
            
            return request.Result; 
        }

        public async Task<IList<DealProductItem>> GetProductsForDeal(uint dealId)
        {
            AddJsonHeader();
            string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.deal.productrows.get.json?id={dealId}";
            var msg = client.GetStringAsync(requestUri);
            await semaphoreSlim.WaitAsync();

            DealProductItemResponse request = null;
            try{
                logger.Info("Ждем ProductFromDeal");

                Thread.Sleep(1000);
                request = JsonConvert.DeserializeObject<DealProductItemResponse>(await msg);
                logger.Info("Подождали ProductFromDeal");

            }
            finally{
                semaphoreSlim.Release();
            }
            
            return request.Result; 
        }

		/*
        public async Task<IList<uint>> GetDealsIdsBetweenDates(DateTime date1, DateTime date2)
        {
            string date1Formatted = date1.ToString("dd.MM.yyyy HH:mm:ss");
            string date2Formatted = date2.ToString("dd.MM.yyyy HH:mm:ss");
            IList<uint> listOfIds = new List<uint>();
            AddJsonHeader();
            DealsResponse request;
            uint i = 0;
            bool needOneMoretime = false;
            do{
                string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.deal.list.json?" +
                                    $"FILTER[>DATE_CREATE]={date1Formatted}&" +
                                    $"FILTER[<DATE_CREATE]={date2Formatted}&" +
                                    $"FILTER[STAGE_ID]={createInDVStageId}&" +
                                    $"start={50 * i}";
                request = JsonConvert.DeserializeObject<DealsResponse>(await client.GetStringAsync(requestUri));
                if (request.Total > 50)
                    needOneMoretime = true;
                
                foreach (var dealFromList in request.Result)
                    listOfIds.Add(dealFromList.Id);
                
                ++i;
            } while (request.Next != null);

            if(needOneMoretime){
                string requestUri2 = $"{baseURL}/rest/{userId}/{token}/crm.deal.list.json?" +
                                     $"FILTER[>DATE_MODIFY]={date1Formatted}&" +
                                     $"FILTER[<DATE_MODIFY]={date2Formatted}&" +
                                     $"FILTER[STAGE_ID]={createInDVStageId}&" +
                                     $"start={50 * i}";
                request = JsonConvert.DeserializeObject<DealsResponse>(await client.GetStringAsync(requestUri2));
                foreach (var dealFromList in request.Result)
                    listOfIds.Add(dealFromList.Id);
            }
            
         
            logger.Info($"В период между {date1} и {date2} получено {listOfIds.Count} сделок в статусе завести в ДВ" +
                        "\nДесериализация в DTO...");
            return listOfIds;
        }
		*/

		public IList<Deal> GetDeals(DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			using(var httpClient = CreateHttpClient())
			{
				List<Deal> deals = new List<Deal>();

				string dateFrom = dateTimeFrom.ToString("dd.MM.yyyy HH:mm:ss");
				string dateTo = dateTimeTo.ToString("dd.MM.yyyy HH:mm:ss");

				int next = 0;
				bool hasNext = true;
				do
				{
					string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.deal.list.json?" +
						$"FILTER[>DATE_CREATE]={dateFrom}&" +
						$"FILTER[<DATE_CREATE]={dateTo}&" +
						//Включаем сделки только со временем доставки
						$"FILTER[>UF_CRM_5DA9BBA03A12A]=0&" +
						//Включаем сделки только в статусе Завести в ДВ
						$"FILTER[STAGE_ID]={createInDVStageId}" +
						//Добавляем в выгрузку все пользовательские поля
						$"&select[]=*&select[]=UF_*" +
						$"&start={next}";

					var requestTask = httpClient.GetStringAsync(requestUri);
					requestTask.Wait();
					var responseString = requestTask.Result;
					DealsResponse response;
					try
					{
						response = JsonConvert.DeserializeObject<DealsResponse>(responseString);
					}
					catch(Exception e)
					{
						logger.Error(e, "Не удалось распарсить одну из сделок");
						continue;
					}

					if(response == null || response.Result == null)
					{
						continue;
					}

					deals.AddRange(response.Result);

					logger.Info($"Загружено сделок {deals.Count}/{response.Total}");
					hasNext = response.Next.HasValue;
					next = response.Next.HasValue ? response.Next.Value : 0;
				} while(hasNext);

				return deals;
			}
		}


		public async Task<bool> SendWONBitrixStatus(uint bitrixId)
        {
            return true; //TODO test
            
            //Задержка в 10сек, это необходимо из-за битрикса, он не успевает обработать какие-то скрипты
            Thread.Sleep(10000);
            AddJsonHeader();
            string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.deal.update.json?id={bitrixId}&FIELDS[STAGE_ID]=WON";
            var msg = client.GetStringAsync(requestUri);
            
            await semaphoreSlim.WaitAsync();

            ChangeStatusResult request = null;
            try{
                logger.Info("Ждем ProductFromDeal");

                Thread.Sleep(1000);
                request = JsonConvert.DeserializeObject<ChangeStatusResult>(await msg);
                logger.Info("Подождали ProductFromDeal");
            }
            finally{
                semaphoreSlim.Release();
            }
            
            return request.Result; 
        }

        #region CustomFields

		/*
        //crm.deal.userfield.list
        public async Task<IList<CustomFieldFromList>> GetAllCustomFieldsFromDeal()
        {
            AddJsonHeader();
            string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.deal.userfield.list.json";
            var msg = client.GetStringAsync(requestUri);
            
            CustomFieldsDealList request = null;
            try{
                logger.Info("Ждем AllCustomFieldsFromDeal");
                Thread.Sleep(1000);
                request = JsonConvert.DeserializeObject<CustomFieldsDealList>(await msg);
                logger.Info("Подождали ProductFromDeal");
            }
            finally{
                semaphoreSlim.Release();
            }
            
            return request.Result; 
        }
		*/
        
		/*
        //crm.deal.userfield.get
        public async Task<CustomField> GetCustomFieldDeal(int id)
        {
            AddJsonHeader();
            string requestUri = $"{baseURL}/rest/{userId}/{token}/crm.deal.userfield.get.json?id={id}";
            var msg = client.GetStringAsync(requestUri);
            
            CustomFileldDealItem request = null;
            try{
                Thread.Sleep(1000);
                request = JsonConvert.DeserializeObject<CustomFileldDealItem>(await msg);
            }
            finally{
                semaphoreSlim.Release();
            }
            
            return request.Result; 
        }
		*/

		/*
        public async Task<Dictionary<string, CustomField>> GetMapCustomFieldsShitNamesToRus()
        {
            var map = new Dictionary<string, CustomField>();
            var customFieldsList = await GetAllCustomFieldsFromDeal();
            foreach (var customField in customFieldsList.Take(5)) //TODO gavr убрать take 5
            {
                map[customField.FieldName] = await GetCustomFieldDeal(customField.Id);
                Thread.Sleep(1000);
            }
            return map;
        }

        
        public string SerializeCustomFieldsShitToRusNamesToFile(Dictionary<string, CustomField> ShitToRusNames)
        {
            var builder = new StringBuilder();
            foreach (var shitToRusName in ShitToRusNames)
            {
                builder.Append(shitToRusName.Key);
                builder.Append(":");
                builder.Append(shitToRusName.Value.RussianName.Name);
                builder.Append('\n');
            }
            builder.Append('\n');
            return builder.ToString();
        }

        public async Task CreateShitToRusCustomFieldsFile(string filename)
        {
            if (!File.Exists(filename))
            {
                var shitNamesToRus = await GetMapCustomFieldsShitNamesToRus();
                var text = SerializeCustomFieldsShitToRusNamesToFile(shitNamesToRus);
                //Добавляем дату в имя файла на случай если файл будет пересоздаваться раз в N дней
                var filenameWithDate = filename + DateTime.Now.ToString("d") + ".txt";
                File.WriteAllText(filenameWithDate, text);
            }
        }
		*/

        #endregion CustomFields
        
        private static void AddJsonHeader()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

		private HttpClient CreateHttpClient()
		{
			HttpClient client = new HttpClient();

			client.DefaultRequestHeaders.Accept.Clear();
			var jsonHeader = new MediaTypeWithQualityHeaderValue("application/json");
			client.DefaultRequestHeaders.Accept.Add(jsonHeader);

			return client;
		}

	}
}