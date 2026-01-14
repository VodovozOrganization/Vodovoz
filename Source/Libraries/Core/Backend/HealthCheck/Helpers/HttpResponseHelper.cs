using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VodovozHealthCheck.Helpers
{
	/// <summary>
	///		Утилитный класс для отправки HTTP-запросов и обработки ответов.
	///		Содержит методы для получения JSON/объектов по URI и универсальный метод отправки запроса с десериализацией в целевой тип.
	/// </summary>
	public static class HttpResponseHelper
	{
		// Заголовок для идентификации проверки работоспособности
		private const string HealthCheckHeader = "X-Health-Check";

		private static readonly JsonSerializerOptions VodovozDefaultJsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		/// <summary>
		///		Универсальный метод получения <see cref="HttpResponseMessage"/> по URI.
		///		Поддерживает Bearer авторизацию и передачу API-ключа в заголовке.
		/// </summary>
		/// <param name="requestUri">URI запроса.</param>
		/// <param name="httpClientFactory">Фабрика для получения <see cref="HttpClient"/>.</param>
		/// <param name="bearerAccessToken">JWT/Bearer токен для авторизации (опционально).</param>
		/// <param name="apiKey">Имя заголовка API-ключа (опционально).</param>
		/// <param name="apiKeyValue">Значение API-ключа (опционально).</param>
		/// <param name="cancellationToken">Токен отмены операции.</param>
		/// <returns>Экземпляр <see cref="HttpResponseMessage"/>.</returns>
		public static async Task<HttpResponseMessage> GetByUriAsync(
			string requestUri,
			IHttpClientFactory httpClientFactory,
			string bearerAccessToken = null,
			string apiKey = null,
			string apiKeyValue = null,
			CancellationToken cancellationToken = default)
		{
			var httpClient = httpClientFactory.CreateClient();

			if(bearerAccessToken != null)
			{
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerAccessToken);
			}

			if(!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiKeyValue))
			{
				httpClient.DefaultRequestHeaders.Add(apiKey, apiKeyValue);
			}

			var responseMessage = await httpClient.GetAsync(requestUri, cancellationToken);

			return responseMessage;
		}

		/// <summary>
		///		Универсальный метод получения JSON-ответа по URI и десериализации в <typeparamref name="TResponse"/>.
		///		Поддерживает Bearer авторизацию и передачу API-ключа в заголовке.
		/// </summary>
		/// <typeparam name="TResponse">Тип, в который будет десериализован ответ.</typeparam>
		/// <param name="requestUri">URI запроса.</param>
		/// <param name="httpClientFactory">Фабрика для получения <see cref="HttpClient"/>.</param>
		/// <param name="bearerAccessToken">JWT/Bearer токен для авторизации (опционально).</param>
		/// <param name="apiKey">Имя заголовка API-ключа (опционально).</param>
		/// <param name="apiKeyValue">Значение API-ключа (опционально).</param>
		/// <param name="cancellationToken">Токен отмены операции.</param>
		/// <param name="jsonSerializerOptions">Опции JSON-сериализации/десериализации. Если null — используются VodovozDefaultJsonOptions.</param>
		/// <returns>Десериализованный объект типа <typeparamref name="TResponse"/>.</returns>
		public static async Task<TResponse> GetJsonByUriAsync<TResponse>(
			string requestUri,
			IHttpClientFactory httpClientFactory,
			string bearerAccessToken = null,
			string apiKey = null,
			string apiKeyValue = null,
			CancellationToken cancellationToken = default,
			JsonSerializerOptions jsonSerializerOptions = null)
		{
			var httpClient = httpClientFactory.CreateClient();

			if(bearerAccessToken != null)
			{
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerAccessToken);
			}

			if(!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiKeyValue))
			{
				httpClient.DefaultRequestHeaders.Add(apiKey, apiKeyValue);
			}

			var options = jsonSerializerOptions ?? VodovozDefaultJsonOptions;

			return await httpClient.GetFromJsonAsync<TResponse>(requestUri, options, cancellationToken);
		}

		/// <summary>
		///		Универсальный метод отправки HTTP-запроса с поддержкой:
		///		- Bearer авторизации (accessToken)
		///		- передачи API-ключа в заголовках (apiKey, apiKeyValue)
		///		- пометки запроса как health-check (добавляется заголовок X-Health-Check)
		///		Возвращает <see cref="HttpResponseWrapper{TResponse}"/> с десериализованными данными или сообщением об ошибке.
		/// </summary>
		/// <typeparam name="TResponse">Тип ожидаемого содержимого ответа. Для text/plain используйте <see cref="string"/>.</typeparam>
		/// <param name="httpMethod">HTTP-метод запроса.</param>
		/// <param name="requestUri">URI ресурса.</param>
		/// <param name="httpClientFactory">Фабрика для получения <see cref="HttpClient"/>.</param>
		/// <param name="requestContent">Тело запроса (<see cref="HttpContent"/>). Передавайте null если тела нет.</param>
		/// <param name="cancellationToken">Токен отмены операции.</param>
		/// <param name="accessToken">JWT/Bearer токен для Authorization (опционально).</param>
		/// <param name="apiKey">Имя заголовка API-ключа (опционально).</param>
		/// <param name="apiKeyValue">Значение API-ключа (опционально).</param>
		/// <param name="isHealthCheck">Если true — в запрос добавляется заголовок X-Health-Check.</param>
		/// <param name="jsonSerializerOptions">Опции JSON-сериализации/десериализации. Если null — используются настройки по умолчанию VodovozDefaultJsonOptions.</param>
		/// <returns>
		///		Экземпляр <see cref="HttpResponseWrapper{TResponse}"/>, содержащий код статуса, признак успеха,
		///		десериализованные данные (Data) и текст ошибки (ErrorMessage) при неуспешном ответе или ошибке десериализации.
		/// </returns>
		public static async Task<HttpResponseWrapper<TResponse>> SendRequestAsync<TResponse>(
			HttpMethod httpMethod,
			string requestUri,
			IHttpClientFactory httpClientFactory,
			HttpContent requestContent = null,
			CancellationToken cancellationToken = default,
			string accessToken = null,
			string apiKey = null,
			string apiKeyValue = null,
			bool isHealthCheck = true,
			JsonSerializerOptions jsonSerializerOptions = null)
		{
			using var request = new HttpRequestMessage(httpMethod, requestUri);

			request.Content = requestContent;

			if(accessToken != null)
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}

			if(isHealthCheck)
			{
				request.Headers.Add(HealthCheckHeader, "true");
			}

			var httpClient = httpClientFactory.CreateClient();

			if(!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiKeyValue))
			{
				httpClient.DefaultRequestHeaders.Add(apiKey, apiKeyValue);
			}

			using var response = await httpClient.SendAsync(request, cancellationToken);

			var result = new HttpResponseWrapper<TResponse>
			{
				StatusCode = response.StatusCode,
				IsSuccess = response.IsSuccessStatusCode
			};

			var content = await response.Content.ReadAsStringAsync(cancellationToken);

			if(!response.IsSuccessStatusCode)
			{
				result.ErrorMessage = !string.IsNullOrWhiteSpace(content)
					? content
					: response.ReasonPhrase;

				return result;
			}

			if(string.IsNullOrWhiteSpace(content))
			{
				return result;
			}

			var contentType = response.Content.Headers.ContentType?.MediaType;
			var isPlainText = contentType?.Contains("text/plain") == true;

			if(typeof(TResponse) == typeof(string) && isPlainText)
			{
				result.Data = (TResponse)(object)content;

				return result;
			}

			try
			{
				result.Data = JsonSerializer.Deserialize<TResponse>(
					content,
					jsonSerializerOptions ?? VodovozDefaultJsonOptions);
			}
			catch(JsonException e)
			{
				result.IsSuccess = false;
				result.ErrorMessage = $"Ошибка десериализации: {e.Message}";
			}

			return result;
		}

		/// <summary>
		///		Проверяет существование ресурса по указанному URI, выполняя HEAD-запрос.
		///		Таймаут установлен в 10 секунд.
		/// </summary>
		/// <param name="uri">URI ресурса для проверки.</param>
		/// <param name="httpClientFactory">Фабрика для получения <see cref="HttpClient"/>.</param>
		/// <returns>true, если ответ имеет успешный статус (200-299); иначе false.</returns>
		public static async Task<bool> CheckUriExistsAsync(
			string uri,
			IHttpClientFactory httpClientFactory)
		{
			try
			{
				var httpClient = httpClientFactory.CreateClient();
				httpClient.Timeout = TimeSpan.FromSeconds(10);

				var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri));

				// Только 200-299 = страница точно есть
				return response.IsSuccessStatusCode;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		///		Определяет, является ли текущий HTTP-запрос health-check запросом (проверкой работоспособности).
		/// </summary>
		/// <param name="request">Текущий HTTP-запрос.</param>
		/// <returns>true, если запрос содержит заголовок X-Health-Check.</returns>
		public static bool IsHealthCheckRequest(HttpRequest request)
		{
			return request.Headers.ContainsKey(HealthCheckHeader);
		}
	}
}
