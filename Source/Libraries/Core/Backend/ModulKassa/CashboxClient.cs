using Microsoft.Extensions.Logging;
using ModulKassa.DTO;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModulKassa
{
	public class CashboxClient
	{
		private const string _fiscalizationStatusUrl = "fn/v1/status";
		private const string _documentStatusUrl = "fn/v1/doc/{0}/status";
		private const string _documentRequeueUrl = "fn/v1/doc/{0}/re-queue";
		private const string _sendDocumentUrl = "fn/v2/doc";

		private readonly ILogger<CashboxClient> _logger;
		private readonly CashboxSetting _setting;

		private readonly HttpClient _httpClient;

		public int Id { get; }

		public CashboxClient(ILogger<CashboxClient> logger, CashboxSetting setting)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_setting = setting ?? throw new ArgumentNullException(nameof(setting));

			if(string.IsNullOrWhiteSpace(setting.BaseUrl))
			{
				throw new ArgumentException($"'{nameof(setting.BaseUrl)}' cannot be null or whitespace.", nameof(setting.BaseUrl));
			}

			Id = setting.CashBoxId;

			_httpClient = CreateHttpClient();
		}

		private HttpClient CreateHttpClient()
		{
			var authentication = new AuthenticationHeaderValue(
				"Basic",
				Convert.ToBase64String(
					Encoding.GetEncoding("ISO-8859-1").GetBytes($"{_setting.UserId}:{_setting.Password}")
				)
			);

			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Authorization = authentication;
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.BaseAddress = new Uri(_setting.BaseUrl);
			httpClient.Timeout = TimeSpan.FromSeconds(60);

			return httpClient;
		}

		public async Task<bool> CanFiscalizeAsync(CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation("Проверка фискального регистратора №{cashboxId} ({cashboxName}).",
					_setting.CashBoxId, _setting.RetailPointName);

				var response = await _httpClient.GetAsync(_fiscalizationStatusUrl, cancellationToken);
				if(!response.IsSuccessStatusCode)
				{
					var httpCodeMessage = $"HTTP Code: {(int)response.StatusCode} {response.StatusCode}";
					_logger.LogWarning("Проверка фискального регистратора№{cashboxId} не пройдена. {httpCodeMessage}.",
						_setting.CashBoxId, httpCodeMessage);

					return false;
				}
				var cashboxStatus = await response.Content.ReadAsAsync<CashboxStatus>(cancellationToken);
				if(cashboxStatus == null)
				{
					_logger.LogWarning("Проверка фискального регистратора №{cashboxId} не пройдена. " +
						"Не удалось десериализовать ответ.", _setting.CashBoxId);
					return false;
				}

				switch(cashboxStatus.FiscalRegistratorStatus)
				{
					case FiscalRegistratorStatus.Ready:
						_logger.LogInformation("Проверка фискального регистратора №{cashboxId} проведена успешно. " +
							"Его состояние позволяет фискализировать чеки.", _setting.CashBoxId);
						return true;
					case FiscalRegistratorStatus.Failed:
						_logger.LogWarning("Проблемы получения статуса фискального регистратора №{cashboxId}. " +
							"Этот статус не препятствует добавлению документов для фискализации. " +
							"Все документы будут добавлены в очередь на сервере и дождутся момента когда касса будет " +
							"в состоянии их фискализировать.", _setting.CashBoxId);
						return true;
					case FiscalRegistratorStatus.Associated:
						_logger.LogWarning("Клиент успешно связан с розничной точкой, " +
							"но касса еще ни разу не вышла на связь и не сообщила свое состояние. " +
							"Отправка чеков для фискального регистратора №{cashboxId} отменена", _setting.CashBoxId);
						return _setting.IsTestMode;
					default:
						_logger.LogWarning("Проверка фискального регистратора №{cashboxId} не пройдена. " +
							"{finscalizatorStatusResponse.Message}", _setting.CashBoxId, cashboxStatus.Message);
						return false;
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при проверке фискального регистратора №{cashboxId}", _setting.CashBoxId);
				return false;
			}
		}

		public async Task<FiscalizationResult> SendFiscalDocument(FiscalDocument doc, CancellationToken cancellationToken)
		{
			try
			{
				var result = new FiscalizationResult();

				var response = await _httpClient.PostAsJsonAsync(_sendDocumentUrl, doc, cancellationToken);
				if(!response.IsSuccessStatusCode)
				{
					var httpCodeMessage = $"HTTP Code: {(int)response.StatusCode} {response.StatusCode}";
					_logger.LogWarning("Не удалось отправить фискальный документ №{docId} на кассу №{cashboxId}. {httpCodeMessage}.",
						doc.Id, _setting.CashBoxId, httpCodeMessage);

					_logger.LogWarning("Запуск проверки статуса фискального документа №{docId} на кассу №{cashboxId}", 
						doc.Id, _setting.CashBoxId);

					result = await CheckFiscalDocument(doc, cancellationToken);
					if(result.SendStatus == SendStatus.Error)
					{
						return CreateErrorResult(httpCodeMessage);
					}
					return result;
				}

				var fiscalDocumentInfo = await response.Content.ReadAsAsync<FiscalDocumentInfo>();
				return CreateSucessResult(fiscalDocumentInfo);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при отправке на сервер фискализации");
				return CreateErrorResult(ex.Message);
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
					var errorMessage =
						$"Не удалось получить актуальный статус чека для документа №{fiscalDocumentId}. {httpCodeMessage}";
					_logger.LogWarning(errorMessage);
					return CreateErrorResult(errorMessage);
				}

				var response = await responseContent.Content.ReadAsAsync<FiscalDocumentInfo>();

				return CreateSucessResult(response);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при получении статуса чека для документа №{FiscalDocumentId}",
					fiscalDocumentId);

				return CreateErrorResult(ex.Message);
			}
		}

		public async Task<FiscalizationResult> RequeueFiscalDocument(string fiscalDocumentId, CancellationToken cancellationToken)
		{
			try
			{
				var fiscalizationResult = await CheckFiscalDocument(fiscalDocumentId, cancellationToken);

				if(fiscalizationResult.SendStatus == SendStatus.Success 
					&& fiscalizationResult.FiscalDocumentInfo.Status != FiscalDocumentStatus.Failed)
				{
					return CreateErrorResult(
						$"Для повторного проведения чека статус фискального документа должен быть \"{FiscalDocumentStatus.Failed}\"\n" +
						$"Текущий статус: \"{fiscalizationResult.FiscalDocumentInfo.Status}\"");
				}

				var completedUrl = string.Format(_documentRequeueUrl, fiscalDocumentId);
				var responseContent = await _httpClient.PutAsync(completedUrl, null, cancellationToken);
				if(!responseContent.IsSuccessStatusCode)
				{
					var httpCodeMessage = $"HTTP Code: {(int)responseContent.StatusCode} {responseContent.StatusCode}";
					var errorMessage = $"Не удалось выполнить повторное проведение фискального " +
						$"документа №{fiscalDocumentId}. {httpCodeMessage}";
					_logger.LogWarning(errorMessage);

					return CreateErrorResult(httpCodeMessage);
				}

				var response = await responseContent.Content.ReadAsAsync<FiscalDocumentInfo>();

				return CreateSucessResult(response);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при выполнении повторного проведени фискального документа №{FiscalDocumentId}",
					fiscalDocumentId);

				return CreateErrorResult(ex.Message);
			}
		}

		private FiscalizationResult CreateSucessResult(FiscalDocumentInfo fiscalDocumentInfo)
		{
			var result = new FiscalizationResult
			{
				SendStatus = SendStatus.Success,
				FiscalDocumentInfo = fiscalDocumentInfo,
			};

			if(fiscalDocumentInfo.FailureInfo != null)
			{
				result.ErrorMessage = $"{fiscalDocumentInfo.FailureInfo.Type} : {fiscalDocumentInfo.FailureInfo.Message}";
			}

			return result;
		}

		private FiscalizationResult CreateErrorResult(string message)
		{
			var result = new FiscalizationResult
			{
				SendStatus = SendStatus.Error,
				ErrorMessage = message
			};
			return result;
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}

