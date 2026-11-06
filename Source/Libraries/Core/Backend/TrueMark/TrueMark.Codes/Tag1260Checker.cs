using Grpc.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;

namespace TrueMark.Library
{
	public class Tag1260Checker
	{
		private readonly HttpClient _httpClient;

		private static readonly object _cdnCacheLock = new object();
		private static string _cachedCdnHost;
		private static DateTime _cachedCdnHostExpiration = DateTime.MinValue;
		private static readonly TimeSpan _cdnCacheTtl = TimeSpan.FromMinutes(15);

		private readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			NumberHandling = JsonNumberHandling.AllowReadingFromString,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			PropertyNameCaseInsensitive = true
		};

		protected Tag1260Checker() { }

		public Tag1260Checker(
			IHttpClientFactory httpClientFactory
		)
		{
			if(httpClientFactory is null)
			{
				throw new ArgumentNullException(nameof(httpClientFactory));
			}

			_httpClient = httpClientFactory.CreateClient(nameof(Tag1260Checker));
		}

		/// <summary>
		/// Сброс кэша CDN хоста при сбоях связи
		/// </summary>
		public static void InvalidateCdnCache()
		{
			lock(_cdnCacheLock)
			{
				_cachedCdnHost = null;
				_cachedCdnHostExpiration = DateTime.MinValue;
			}
		}

		private void SetHttpClientHeaderApiKey(Guid headerApiKey)
		{
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_httpClient.DefaultRequestHeaders.Add("X-API-KEY", headerApiKey.ToString());
		}

		private async Task<string> GetCdnAsync(CancellationToken cancellationToken)
		{
			lock(_cdnCacheLock)
			{
				if(!string.IsNullOrEmpty(_cachedCdnHost) && DateTime.UtcNow < _cachedCdnHostExpiration)
				{
					return _cachedCdnHost;
				}
			}

			var uri = "https://cdn.crpt.ru/api/v4/true-api/cdn/info";

			HttpResponseMessage response;
			try
			{
				response = await _httpClient.GetAsync(uri, cancellationToken);
			}
			catch(Exception ex)
			{
				InvalidateCdnCache();
				throw new Tag1260CdnUnavailableException($"Не удалось подключиться к сервису CDN Честного Знака: {ex.Message}", ex);
			}

			if(!response.IsSuccessStatusCode)
			{
				InvalidateCdnCache();
				throw new Tag1260CdnUnavailableException(
					$"Не удалось получить список CDN хостов Честного Знака. HTTP Code: {(int)response.StatusCode} {response.ReasonPhrase}");
			}

			var responseBody = await response.Content.ReadAsStreamAsync();
			var result = await JsonSerializer.DeserializeAsync<CdnInfo>(responseBody, _serializeOptions, cancellationToken);

			if(result?.Hosts == null || !result.Hosts.Any())
			{
				InvalidateCdnCache();
				throw new Tag1260CdnUnavailableException("Ответ сервиса CDN Честного Знака не содержит доступных хостов");
			}

			var cdnHealths = new List<CdnHealth>();

			foreach(var cdnHost in result.Hosts)
			{
				var cdnHealth = await GetAvgTimeMsAsync(cdnHost.Host, cancellationToken);

				if(cdnHealth != null && cdnHealth.Code == 0 && cdnHealth.AvgTimeMs < 1000)
				{
					lock(_cdnCacheLock)
					{
						_cachedCdnHost = cdnHost.Host;
						_cachedCdnHostExpiration = DateTime.UtcNow.Add(_cdnCacheTtl);
					}
					return cdnHost.Host;
				}

				if(cdnHealth != null)
				{
					cdnHealth.Host = cdnHost.Host;
					cdnHealths.Add(cdnHealth);
				}
			}

			var bestHost = cdnHealths.Where(x => x.Code == 0)
				.OrderBy(x => x.AvgTimeMs)
				.FirstOrDefault()?.Host;

			if(string.IsNullOrEmpty(bestHost))
			{
				InvalidateCdnCache();
				throw new Tag1260CdnUnavailableException("Ни один CDN хост Честного Знака не подтвердил доступность (код ответа 0)");
			}

			lock(_cdnCacheLock)
			{
				_cachedCdnHost = bestHost;
				_cachedCdnHostExpiration = DateTime.UtcNow.Add(_cdnCacheTtl);
			}

			return bestHost;
		}

		private async Task<CdnHealth> GetAvgTimeMsAsync(string cdnHost, CancellationToken cancellationToken)
		{
			var uri = $"{cdnHost}/api/v4/true-api/cdn/health/check";

			var response = await _httpClient.GetAsync(uri, cancellationToken);
			if(!response.IsSuccessStatusCode)
			{
				return null;
			}

			var responseBody = await response.Content.ReadAsStreamAsync();
			var result = await JsonSerializer.DeserializeAsync<CdnHealth>(responseBody, _serializeOptions, cancellationToken);

			return result;
		}

		public async Task<IEnumerable<CodeCheckResponse>> CheckCodesWithLimitsForTag1260Async(
			IEnumerable<string> sourceCodes,
			Guid headerApiKey, 
			CancellationToken cancellationToken
			)
		{
			var resultList = new List<CodeCheckResponse>();

			if(!sourceCodes.Any())
			{
				return resultList;
			}

			SetHttpClientHeaderApiKey(headerApiKey);

			var cdn = await GetCdnAsync(cancellationToken);

			var uri = $"{cdn}/api/v4/true-api/codes/check";

			var codesCount = sourceCodes.Count();
			var toSkip = 0;

			while(codesCount > toSkip)
			{
				var sourceCodesToCheck = sourceCodes.Skip(toSkip).Take(100);

				toSkip += 100;
				
				var request = new
				{
					codes = sourceCodesToCheck
				};

				var serializedRequest = JsonSerializer.Serialize(request);
				var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
				var response = await _httpClient.PostAsync(uri, content, cancellationToken);

				if(!response.IsSuccessStatusCode)
				{
					throw new Tag1260ApiException(
						response.StatusCode,
						$"Не удалось проверить коды для разрешительного режима 1260. Code: {(int)response.StatusCode} {response.StatusCode}. {response.ReasonPhrase}");
				}

				var responseBody = await response.Content.ReadAsStreamAsync();

				var result = await JsonSerializer.DeserializeAsync<CodeCheckResponse>(responseBody, _serializeOptions, cancellationToken);

				resultList.Add(result);

				if(toSkip < codesCount)
				{
					await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
				}
			}

			return resultList;
		}

		public virtual async Task<CodeCheckResponse> CheckCodesForTag1260Async(
			IEnumerable<string> sourceCodes,
			Guid headerApiKey,
			CancellationToken cancellationToken
			)
		{
			var codesList = sourceCodes as IList<string> ?? sourceCodes.ToList();
			if(!codesList.Any())
			{
				return new CodeCheckResponse
				{
					Code = 0,
					Description = "ok",
					Codes = new List<CodeCheckInfo>()
				};
			}

			if(codesList.Count <= 100)
			{
				return await CheckSingleBatchForTag1260Async(codesList, headerApiKey, cancellationToken);
			}

			// Если кодов больше 100, разбиваем на батчи по 100 кодов (лимит True API)
			var combinedResponse = new CodeCheckResponse
			{
				Code = 0,
				Description = "ok",
				Codes = new List<CodeCheckInfo>()
			};

			for(int i = 0; i < codesList.Count; i += 100)
			{
				var batch = codesList.Skip(i).Take(100).ToList();
				var batchResponse = await CheckSingleBatchForTag1260Async(batch, headerApiKey, cancellationToken);

				if(batchResponse == null || batchResponse.Code != 0)
				{
					return batchResponse;
				}

				if(batchResponse.Codes != null)
				{
					foreach(var code in batchResponse.Codes)
					{
						combinedResponse.Codes.Add(code);
					}
				}

				combinedResponse.ReqId = batchResponse.ReqId;
				combinedResponse.ReqTimestamp = batchResponse.ReqTimestamp;
			}

			return combinedResponse;
		}

		private async Task<CodeCheckResponse> CheckSingleBatchForTag1260Async(
			IEnumerable<string> sourceCodes,
			Guid headerApiKey,
			CancellationToken cancellationToken
			)
		{
			SetHttpClientHeaderApiKey(headerApiKey);

			var cdn = await GetCdnAsync(cancellationToken);
			var uri = $"{cdn}/api/v4/true-api/codes/check";

			var request = new
			{
				codes = sourceCodes
			};

			var serializedRequest = JsonSerializer.Serialize(request);
			var content = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

			HttpResponseMessage response;
			try
			{
				response = await _httpClient.PostAsync(uri, content, cancellationToken);
			}
			catch(Exception)
			{
				InvalidateCdnCache();
				throw;
			}

			if(!response.IsSuccessStatusCode)
			{
				if((int)response.StatusCode >= 500)
				{
					InvalidateCdnCache();
				}

				throw new Tag1260ApiException(
					response.StatusCode,
					$"Не удалось проверить коды для разрешительного режима 1260. Code: {(int)response.StatusCode} {response.StatusCode}. {response.ReasonPhrase}");
			}

			var responseBody = await response.Content.ReadAsStreamAsync();
			var result = await JsonSerializer.DeserializeAsync<CodeCheckResponse>(responseBody, _serializeOptions, cancellationToken);

			return result;
		}
	}
}
