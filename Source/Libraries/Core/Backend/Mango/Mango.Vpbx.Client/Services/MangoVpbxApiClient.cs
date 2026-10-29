using Mango.Core.Dto.Vpbx.Responses;
using Mango.Core.Sign;
using Mango.Vpbx.Client.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Mango;

namespace Mango.Vpbx.Client.Services
{
	/// <inheritdoc/>
	public class MangoVpbxApiClient : IMangoVpbxApiClient
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			// Незаполненные необязательные параметры не должны попадать в запрос:
			// например, отсутствие extension означает запрос всех сотрудников ВАТС
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

			// Страховка на случай, если числовое поле придёт строкой. На практике ВАТС возвращает
			// числа числами, но в документации часть примеров ответа показывает order и wait_sec
			// строками, поэтому разбор допускает оба варианта
			NumberHandling = JsonNumberHandling.AllowReadingFromString,

			// Кириллица в ФИО сотрудника уходит как есть, а не в виде \uXXXX
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		private readonly ILogger<MangoVpbxApiClient> _logger;
		private readonly HttpClient _httpClient;
		private readonly ISignGenerator _signGenerator;
		private readonly IMangoSettings _mangoSettings;

		public MangoVpbxApiClient(
			ILogger<MangoVpbxApiClient> logger,
			HttpClient httpClient,
			ISignGenerator signGenerator,
			IMangoSettings mangoSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_signGenerator = signGenerator ?? throw new ArgumentNullException(nameof(signGenerator));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
		}

		/// <inheritdoc/>
		public async Task<TResponse> PostAsync<TRequest, TResponse>(
			string endpoint,
			TRequest request,
			bool resultCodeRequired,
			CancellationToken cancellationToken)
			where TResponse : VpbxResponseBase
		{
			var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);
			var sign = _signGenerator.GetSign(_mangoSettings.VpbxApiKey, _mangoSettings.VpbxApiSalt, json);

			_logger.LogDebug("Запрос к {Endpoint} ВАТС Манго: {Json}", endpoint, json);

			var content = new FormUrlEncodedContent(new Dictionary<string, string>
			{
				["vpbx_api_key"] = _mangoSettings.VpbxApiKey,
				["sign"] = sign,
				["json"] = json
			});

			string responseBody;
			HttpStatusCode statusCode;

			using(content)
			using(var response = await _httpClient.PostAsync(endpoint, content, cancellationToken))
			{
				statusCode = response.StatusCode;
				responseBody = await response.Content.ReadAsStringAsync();
			}

			_logger.LogDebug("Ответ {Endpoint} ВАТС Манго: {ResponseBody}", endpoint, responseBody);

			if((int)statusCode < 200 || (int)statusCode > 299)
			{
				throw CreateExceptionForFailedStatusCode(endpoint, statusCode, responseBody);
			}

			TResponse result;
			try
			{
				result = JsonSerializer.Deserialize<TResponse>(responseBody, _jsonSerializerOptions);
			}
			catch(JsonException e)
			{
				throw new MangoVpbxApiException(
					$"Не удалось разобрать ответ ВАТС Манго на запрос {endpoint}: {e.Message}",
					endpoint,
					statusCode,
					null,
					responseBody);
			}

			if(result is null)
			{
				throw new MangoVpbxApiException(
					$"ВАТС Манго вернула пустой ответ на запрос {endpoint}",
					endpoint,
					statusCode,
					null,
					responseBody);
			}

			if(!result.Result.HasValue)
			{
				// Отсутствие кода результата у метода, который его возвращает, означает,
				// что ответ не распознан. Считать такой ответ успешным нельзя: например,
				// для удаления сотрудника это означало бы, что сотрудник остался в ВАТС
				// и продолжает удерживать свой внутренний номер
				if(resultCodeRequired)
				{
					throw new MangoVpbxApiException(
						$"ВАТС Манго не вернула код результата на запрос {endpoint}",
						endpoint,
						statusCode,
						null,
						responseBody);
				}

				return result;
			}

			if(result.Result.Value != VpbxResultCodes.Success)
			{
				throw new MangoVpbxApiException(
					$"Запрос {endpoint} к ВАТС Манго завершился с кодом результата {result.Result.Value}"
						+ GetErrorDetails(result),
					endpoint,
					statusCode,
					result.Result,
					responseBody);
			}

			return result;
		}

		/// <summary>
		/// Собирает пояснение к коду результата: описание ошибки и параметры запроса,
		/// к которым она относится. Оба поля заполняются не всегда
		/// </summary>
		private static string GetErrorDetails(VpbxResponseBase response)
		{
			var details = new List<string>();

			if(!string.IsNullOrWhiteSpace(response.Description))
			{
				details.Add(response.Description);
			}

			if(response.Fields.HasValue)
			{
				details.Add($"параметры запроса: {response.Fields.Value.GetRawText()}");
			}

			return details.Count == 0 ? string.Empty : $": {string.Join(", ", details)}";
		}

		/// <summary>
		/// Формирует исключение по ответу с неуспешным HTTP-статусом.
		/// При превышении лимита количества запросов ВАТС отдаёт тело в отдельном формате,
		/// без кода результата, поэтому оно разбирается отдельно
		/// </summary>
		private static MangoVpbxApiException CreateExceptionForFailedStatusCode(
			string endpoint,
			HttpStatusCode statusCode,
			string responseBody)
		{
			if((int)statusCode == 429)
			{
				var error = TryDeserialize<VpbxErrorResponse>(responseBody);

				var errorMessage = string.IsNullOrWhiteSpace(error?.Message)
					? "Rate limit exceeded."
					: error.Message;

				return new MangoVpbxApiException(
					$"Запрос {endpoint} отклонён ВАТС Манго из-за превышения лимита количества запросов: {errorMessage}",
					endpoint,
					statusCode,
					VpbxResultCodes.RateLimitExceeded,
					responseBody);
			}

			return new MangoVpbxApiException(
				$"Запрос {endpoint} к ВАТС Манго завершился с HTTP-статусом {(int)statusCode}",
				endpoint,
				statusCode,
				TryDeserialize<VpbxCommandResponse>(responseBody)?.Result,
				responseBody);
		}

		/// <summary>
		/// Разбирает тело ответа, не бросая исключений: тело неуспешного ответа
		/// не обязано быть корректным json
		/// </summary>
		private static T TryDeserialize<T>(string responseBody)
			where T : class
		{
			if(string.IsNullOrWhiteSpace(responseBody))
			{
				return null;
			}

			try
			{
				return JsonSerializer.Deserialize<T>(responseBody, _jsonSerializerOptions);
			}
			catch(JsonException)
			{
				return null;
			}
		}
	}
}
