using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	/// <summary>
	/// Класс, отправляющий уже подготовленные и проверенные документы и чеки
	/// </summary>
	public class CashboxClient : ICashboxClient
	{
		private readonly ILogger<CashboxClient> _logger;
		private readonly CashboxSetting _cashBox;
		private readonly string _baseUrl;
		private readonly string _fiscalizationStatusUrl;
		private readonly string _documentStatusUrl;
		private readonly string _sendDocumentUrl;
		private readonly HttpClient _httpClient;

		public CashboxClient(ILogger<CashboxClient> logger, CashboxSetting cashBox, string baseUrl)
		{
			if(string.IsNullOrWhiteSpace(baseUrl))
			{
				throw new ArgumentException($"'{nameof(baseUrl)}' cannot be null or whitespace.", nameof(baseUrl));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_cashBox = cashBox ?? throw new ArgumentNullException(nameof(cashBox));
			_baseUrl = baseUrl;

			_fiscalizationStatusUrl = _baseUrl + "fn/v1/status";
			_documentStatusUrl = _baseUrl + "fn/v1/doc/{0}/status";
			_sendDocumentUrl = _baseUrl + "fn/v2/doc";

			_httpClient = CreateHttpClient();
		}

		public int CashboxId => _cashBox.Id;

		public bool IsTestMode { get; set; }

		//public PreparedReceiptNode[] SendReceipts(PreparedReceiptNode[] preparedReceiptNodes, CancellationToken cancellationToken, uint timeoutInSeconds = 300)
		//{
		//	//Необходимо разбить клиент индивидуально на каждую кассу

		//	//Группировка по кассовым аппаратам
		//	var groupedNodes = preparedReceiptNodes.GroupBy(
		//		x => x.CashBox,
		//		(machine, receiptNodes) =>
		//			new
		//			{
		//				machine,
		//				receiptNodes
		//			}
		//	);

		//	IList<Task> runningTasks = new List<Task>();

		//	foreach(var groupedNode in groupedNodes)
		//	{
		//		runningTasks.Add(Task.Run(() =>
		//		{
		//			SendAllDocumentsForCashMachine(groupedNode.machine, groupedNode.receiptNodes.ToList(), cancellationToken);
		//		}, cancellationToken));
		//	}

		//	try
		//	{
		//		Task.WaitAll(runningTasks.ToArray());
		//		return preparedReceiptNodes;
		//	}
		//	catch(Exception ex)
		//	{
		//		logger.Error(ex, "Неизвестная ошибка при отправке чеков");
		//		return preparedReceiptNodes;
		//	}
		//}

		/*private void SendAllDocumentsForCashMachine(CashBox cashBox, IList<PreparedReceiptNode> receiptNodes, CancellationToken cancellationToken)
		{
			logger.Info($"Отправка чеков для фискального регистратора №{cashBox.Id}");

			using(var httpClient = GetHttpClient(cashBox))
			{
				if(!ConnectToCashBox(httpClient, cashBox))
				{
					return;
				}

				int i = 1;
				foreach(var receiptNode in receiptNodes)
				{
					if(cancellationToken.IsCancellationRequested)
					{
						return;
					}

					logger.Info($"Отправка документа №{receiptNode.FiscalDocument.InternalDocumentId} на кассовый аппарат №{cashBox.Id} {cashBox.RetailPointName} ({i++}/{receiptNodes.Count})...");
					receiptNode.FiscalizationResult = SendFiscalDocument(httpClient, receiptNode.FiscalDocument);
				}
			}
		}*/

		private HttpClient CreateHttpClient()
		{
			var authentication = new AuthenticationHeaderValue(
				"Basic",
				Convert.ToBase64String(
					Encoding.GetEncoding("ISO-8859-1").GetBytes($"{_cashBox.UserId}:{_cashBox.Password}")
				)
			);

			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Authorization = authentication;
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.BaseAddress = new Uri(_baseUrl);
			httpClient.Timeout = TimeSpan.FromSeconds(60);

			return httpClient;
		}

		public async Task<bool> CanFiscalizeAsync(CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation($"Проверка фискального регистратора №{_cashBox.Id} ({_cashBox.RetailPointName}).");
				var response = await _httpClient.GetAsync(_fiscalizationStatusUrl, cancellationToken);
				if(!response.IsSuccessStatusCode)
				{
					var httpCodeMessage = $"HTTP Code: {(int)response.StatusCode} {response.StatusCode}";
					_logger.LogWarning($"Проверка фискального регистратора№{_cashBox.Id} не пройдена. {httpCodeMessage}.");
					return false;
				}

				var finscalizatorStatusResponse = await response.Content.ReadAsAsync<CashboxStatusResponse>(cancellationToken);
				if(finscalizatorStatusResponse == null)
				{
					_logger.LogWarning($"Проверка фискального регистратора №{_cashBox.Id} не пройдена. Не удалось десериализовать ответ.");
					return false;
				}

				switch(finscalizatorStatusResponse.CashboxStatus)
				{
					case FiscalRegistratorStatus.Ready:
						_logger.LogInformation($"Проверка фискального регистратора №{_cashBox.Id} проведена успешно. Его состояние позволяет фискализировать чеки.");
						return true;
					case FiscalRegistratorStatus.Failed:
						_logger.LogWarning($"Проблемы получения статуса фискального регистратора №{_cashBox.Id}. " +
							"Этот статус не препятствует добавлению документов для фискализации. " +
							"Все документы будут добавлены в очередь на сервере и дождутся момента когда касса будет в состоянии их фискализировать.");
						return true;
					case FiscalRegistratorStatus.Associated:
						_logger.LogWarning("Клиент успешно связан с розничной точкой, " +
							$"но касса еще ни разу не вышла на связь и не сообщила свое состояние. " +
							$"Отправка чеков для фискального регистратора №{_cashBox.Id} отменена");
						return IsTestMode;
					default:
						_logger.LogWarning($"Проверка фискального регистратора №{_cashBox.Id} не пройдена. {finscalizatorStatusResponse.Message}");
						return false;
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при проверке фискального регистратора №{_cashBox.Id}");
				return false;
			}
		}

		public async Task<FiscalizationResult> SendFiscalDocument(FiscalDocument doc, CancellationToken cancellationToken)
		{
			try
			{
				var result = new FiscalizationResult();

				var responseContent = await _httpClient.PostAsJsonAsync(_sendDocumentUrl, doc, cancellationToken);
				if(!responseContent.IsSuccessStatusCode)
				{
					result = await CheckFiscalDocument(doc, cancellationToken);
					return result;
				}

				var response = await responseContent.Content.ReadAsAsync<FiscalDocumentInfoResponse>();
				return CreateSucessResult(response);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке на сервер фискализации");
				return CreateFailResult(ex.Message);
			}
		}

		public async Task<FiscalizationResult> CheckFiscalDocument(FiscalDocument doc, CancellationToken cancellationToken)
		{
			return await CheckFiscalDocument(doc.Id, cancellationToken);
		}

		public async Task<FiscalizationResult> CheckFiscalDocument(string fiscalDocumentId, CancellationToken cancellationToken)
		{
			try
			{
				var completedUrl = string.Format(_documentStatusUrl, fiscalDocumentId);
				var responseContent = await _httpClient.GetAsync(completedUrl, cancellationToken);
				if(!responseContent.IsSuccessStatusCode)
				{
					var httpCodeMessage = $"HTTP Code: {(int)responseContent.StatusCode} {responseContent.StatusCode}";
					_logger.LogWarning($"Не удалось получить актуальный статус чека для документа №{fiscalDocumentId}. {httpCodeMessage}");
					return CreateFailResult(httpCodeMessage);
				}

				var response = await responseContent.Content.ReadAsAsync<FiscalDocumentInfoResponse>();
				return CreateSucessResult(response);

			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при получении статуса чека для документа №{fiscalDocumentId}");
				return CreateFailResult(ex.Message);
			}
		}

		private FiscalizationResult CreateSucessResult(FiscalDocumentInfoResponse response)
		{
			var result = new FiscalizationResult
			{
				SendStatus = SendStatus.Success,
				Status = response.Status,
				StatusChangedTime = DateTime.Parse(response.TimeStatusChangedString)
			};

			if(response.FiscalInfo != null)
			{
				result.FiscalDocumentNumber = response.FiscalInfo.FnDocNumber;
				result.FiscalDocumentDate = DateTime.Parse(response.FiscalInfo.Date);
			}

			if(response.FailureInfo != null)
			{
				result.FailDescription = $"{response.FailureInfo.Type} : {response.FailureInfo.Message}";
			}

			return result;
		}

		private FiscalizationResult CreateFailResult(string failDescription)
		{
			var result = new FiscalizationResult
			{
				SendStatus = SendStatus.Error,
				FailDescription = failDescription
			};
			return result;
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}
