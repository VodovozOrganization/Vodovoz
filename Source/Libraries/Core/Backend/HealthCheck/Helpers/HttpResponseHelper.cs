using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace VodovozHealthCheck.Helpers
{
	/// <summary>
	///		Утилитный класс для отправки HTTP-запросов и обработки ответов.
	/// </summary>
	public static class HttpResponseHelper
	{
		/// <summary>
		/// Заголовок для идентификации проверки работоспособности
		/// </summary>
		private const string _healthCheckHeader = "X-Health-Check";
		private static readonly HttpRequestOptionsKey<string> _сonnectInfoKey =new("connect-info");

		/// <summary>
		/// Таймаут health-check запроса
		/// </summary>
		private const int _healthCheckTimeoutSeconds = 25;

		private static readonly JsonSerializerOptions _vodovozDefaultJsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		/// <summary>
		/// HttpClient с диагностикой соединений
		/// </summary>
		private static readonly HttpClient _diagnosticHttpClient = CreateDiagnosticClient();

		/// <summary>
		/// Создаёт HttpClient с SocketsHttpHandler и логированием подключения
		/// </summary>
		/// <returns>HttpClient</returns>
		private static HttpClient CreateDiagnosticClient()
		{
			var handler = new SocketsHttpHandler
			{
				MaxConnectionsPerServer = Environment.ProcessorCount * 8,

				PooledConnectionIdleTimeout = TimeSpan.FromSeconds(_healthCheckTimeoutSeconds),
				PooledConnectionLifetime = TimeSpan.FromMinutes(5),

				AutomaticDecompression =
					DecompressionMethods.GZip |
					DecompressionMethods.Deflate,

				ConnectCallback = async (context, cancellationToken) =>
				{
					var sw = Stopwatch.StartNew();
					var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

					try
					{
						await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
						sw.Stop();

						context.InitialRequestMessage?.Options.Set(
							_сonnectInfoKey,
							$"ConnectedIP={socket.RemoteEndPoint}; ConnectMs={sw.ElapsedMilliseconds}");

						return new NetworkStream(socket, ownsSocket: true);
					}
					catch
					{
						socket.Dispose();
						throw;
					}
				}
			};

			return new HttpClient(handler)
			{
				Timeout = TimeSpan.FromSeconds(_healthCheckTimeoutSeconds)
			};
		}

		/// <summary>
		/// Возвращает полный стек исключений с деталями SocketException, включая все InnerException
		/// </summary>
		/// <param name="ex">Исключение</param>
		/// <returns>Подробная информация об исключении</returns>
		private static string GetFullException(Exception ex)
		{
			var sb = new StringBuilder();
			int level = 0;

			while(ex != null)
			{
				var indent = new string('-', level * 4);

				sb.AppendLine($"{indent}{ex.GetType().Name}: {ex.Message}");

				if(ex is SocketException sex)
				{
					sb.AppendLine($"{indent}SocketException Details:");
					sb.AppendLine($"{indent}  NativeErrorCode={sex.NativeErrorCode}");
					sb.AppendLine($"{indent}  SocketErrorCode={sex.SocketErrorCode}");
					sb.AppendLine($"{indent}  ErrorCode={sex.ErrorCode}");
					if(sex.Data != null && sex.Data.Count > 0)
					{
						sb.AppendLine($"{indent}  Data:");
						foreach(System.Collections.DictionaryEntry de in sex.Data)
						{
							sb.AppendLine($"{indent}    {de.Key} = {de.Value}");
						}
					}
				}

				ex = ex.InnerException;

				if(ex != null)
				{
					sb.AppendLine($"{indent}---- INNER ----");
				}

				level++;
			}

			return sb.ToString();
		}

		/// <summary>
		/// Возвращает IP-адреса сетевых интерфейсов контейнера
		/// </summary>
		/// <returns>Строка с IP контейнера</returns>
		private static string GetContainerNetworkInfo()
		{
			try
			{
				var interfaces = NetworkInterface.GetAllNetworkInterfaces();
				var sb = new StringBuilder();

				foreach(var ni in interfaces)
				{
					var ips = ni.GetIPProperties()
						.UnicastAddresses
						.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
						.Select(a => a.Address.ToString());

					foreach(var ip in ips)
					{
						sb.Append($"{ni.Name}:{ip} ");
					}
				}

				return sb.ToString();
			}
			catch
			{
				return "NetworkInfo unavailable";
			}
		}

		/// <summary>
		/// Возвращает состояние ThreadPool
		/// </summary>
		/// <returns>Информация о потоках</returns>
		private static string GetThreadPoolInfo()
		{
			ThreadPool.GetAvailableThreads(out var worker, out var io);
			ThreadPool.GetMaxThreads(out var maxWorker, out var maxIo);
			return $"ThreadPool worker={worker}/{maxWorker} io={io}/{maxIo}";
		}

		/// <summary>
		/// Формирует диагностическую информацию запроса
		/// </summary>
		/// <param name="requestUri">URI запроса</param>
		/// <param name="dnsInfo">Информация о DNS</param>
		/// <param name="durationMs">Время выполнения в мс</param>
		/// <param name="request">HttpRequestMessage</param>
		/// <returns>Строка диагностики</returns>
		private static string BuildDiagnosticInfo(string requestUri, long durationMs, HttpRequestMessage request)
		{
			var uri = new Uri(requestUri);

			// Пытаемся получить диагностическую информацию о TCP-подключении,
			// которую ранее сохранили в ConnectCallback.
			// Если соединение было взято из пула HttpClient, значения может не быть.
			request.Options.TryGetValue(
				_сonnectInfoKey,
				out var connectInfo);

			connectInfo ??= "ConnectInfo=pooled-connection";

			return $"RequestUri={requestUri}; Host={uri.Host}; Port={uri.Port}; Scheme={uri.Scheme}; {connectInfo}; DurationMs={durationMs}; ContainerNetwork={GetContainerNetworkInfo()}; {GetThreadPoolInfo()}";
		}

		/// <summary>
		/// Универсальный метод отправки HTTP-запроса с десериализацией и полной диагностикой
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
		/// <param name="jsonSerializerOptions">Опции JSON-сериализации/десериализации. Если null — используются настройки по умолчанию _vodovozDefaultJsonOptions.</param>
		/// <returns>Экземпляр <see cref="HttpResponseWrapper{TResponse}"/> с данными и диагностикой</returns>
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
			var stopwatch = Stopwatch.StartNew();

			using var request = new HttpRequestMessage(httpMethod, requestUri);
			request.Content = requestContent;

			if(accessToken != null)
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			}

			if(isHealthCheck)
			{
				request.Headers.Add(_healthCheckHeader, true.ToString());
			}

			if(!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiKeyValue))
			{
				request.Headers.Add(apiKey, apiKeyValue);
			}

			var result = new HttpResponseWrapper<TResponse>();

			try
			{
				using var response = await _diagnosticHttpClient.SendAsync(request, cancellationToken);
				var content = await response.Content.ReadAsStringAsync(cancellationToken);

				stopwatch.Stop();
				result.IsSuccess = response.IsSuccessStatusCode;
				result.StatusCode = response.StatusCode;

				if(!response.IsSuccessStatusCode)
				{
					result.ErrorMessage = (!string.IsNullOrWhiteSpace(content) ? content : response.ReasonPhrase) + " | " + BuildDiagnosticInfo(requestUri, stopwatch.ElapsedMilliseconds, request);
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
					result.Data = JsonSerializer.Deserialize<TResponse>(content, jsonSerializerOptions ?? _vodovozDefaultJsonOptions);
				}
				catch(JsonException e)
				{
					result.IsSuccess = false;
					result.ErrorMessage = $"Ошибка десериализации: {e.Message} | " + BuildDiagnosticInfo(requestUri, stopwatch.ElapsedMilliseconds, request);
				}

				return result;
			}
			catch(Exception ex)
			{
				stopwatch.Stop();
				result.IsSuccess = false;
				result.ErrorMessage = $"Ошибка сети: {GetFullException(ex)} | " + BuildDiagnosticInfo(requestUri, stopwatch.ElapsedMilliseconds, request);
				return result;
			}
		}

		/// <summary>
		/// Проверяет существование ресурса по указанному URI
		/// </summary>
		/// <param name="uri">URI ресурса для проверки.</param>
		/// <param name="httpClientFactory">Фабрика для получения <see cref="HttpClient"/>.</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <param name="isHealthCheck">Если true — добавляется заголовок X-Health-Check</param>
		/// <returns>Экземпляр <see cref="HttpResponseWrapper{string}"/> с результатом</returns>
		public static async Task<HttpResponseWrapper<string>> CheckUriExistsAsync(
			string uri,
			IHttpClientFactory httpClientFactory,
			CancellationToken cancellationToken,
			bool isHealthCheck = true)
		{
			var stopwatch = Stopwatch.StartNew();

			HttpRequestMessage request = null;
			try
			{
				request = new HttpRequestMessage(HttpMethod.Get, uri);

				if(isHealthCheck)
				{
					request.Headers.Add(_healthCheckHeader, true.ToString());
				}

				using var response = await _diagnosticHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
				stopwatch.Stop();

				return new HttpResponseWrapper<string>
				{
					IsSuccess = response.IsSuccessStatusCode,
					StatusCode = response.StatusCode,
					ErrorMessage = response.ReasonPhrase + " | " + BuildDiagnosticInfo(uri, stopwatch.ElapsedMilliseconds, request)
				};
			}
			catch(Exception ex)
			{
				stopwatch.Stop();
				return new HttpResponseWrapper<string>
				{
					IsSuccess = false,
					ErrorMessage = $"Ошибка сети: {GetFullException(ex)} | " + BuildDiagnosticInfo(uri, stopwatch.ElapsedMilliseconds, request)
				};
			}
			finally
			{
				request?.Dispose();
			}
		}

		/// <summary>
		/// Определяет, является ли текущий HTTP-запрос health-check запросом (проверкой работоспособности)
		/// </summary>
		/// <param name="request">Текущий HTTP-запрос</param>
		/// <returns>true, если запрос содержит заголовок X-Health-Check</returns>
		public static bool IsHealthCheckRequest(HttpRequest request)
		{
			if(request.Headers.TryGetValue(_healthCheckHeader, out var headerValue))
			{
				return bool.TryParse(headerValue, out var result) && result;
			}

			return false;
		}
	}
}
