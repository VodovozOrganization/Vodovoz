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
using TrueMark.Contracts.Responses;

namespace TrueMark.Library
{
	public class Tag1260Checker
	{
		private readonly HttpClient _httpClient;

		private readonly JsonSerializerOptions _serializeOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			NumberHandling = JsonNumberHandling.AllowReadingFromString,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			PropertyNameCaseInsensitive = true
		};

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

		private void SetHttpClientHeaderApiKey(Guid headerApiKey)
		{
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_httpClient.DefaultRequestHeaders.Add("X-API-KEY", headerApiKey.ToString());
		}

		private async Task<string> GetCdnAsync(CancellationToken cancellationToken)
		{
			var uri = "https://cdn.crpt.ru/api/v4/true-api/cdn/info";

			var response = await _httpClient.GetAsync(uri);
			var responseBody = await response.Content.ReadAsStreamAsync();
			var result = await JsonSerializer.DeserializeAsync<CdnInfo>(responseBody, _serializeOptions, cancellationToken);

			var cdnHealths = new List<CdnHealth>();

			foreach(var cdnHost in result.Hosts)
			{
				var cdnHealth = await GetAvgTimeMsAsync(cdnHost.Host, cancellationToken);

				if(cdnHealth.Code == 0 && cdnHealth.AvgTimeMs < 1000)
				{
					return cdnHost.Host;
				}

				cdnHealth.Host = cdnHost.Host;

				cdnHealths.Add(cdnHealth);
			}

			return cdnHealths.Where(x => x.Code == 0)?
				.OrderBy(x => x.AvgTimeMs)?
				.FirstOrDefault()?.Host;
		}

		private async Task<CdnHealth> GetAvgTimeMsAsync(string cdnHost, CancellationToken cancellationToken)
		{
			var uri = $"{cdnHost}/api/v4/true-api/cdn/health/check";

			var response = await _httpClient.GetAsync(uri);
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
					throw new Exception(
						$"Не удалось проверить коды для разрешительного режима 1260. Code: {response.StatusCode}. {response.ReasonPhrase}");
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

		public async Task<CodeCheckResponse> CheckCodesForTag1260Async(
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
			var response = await _httpClient.PostAsync(uri, content, cancellationToken);

			if(!response.IsSuccessStatusCode)
			{
				throw new Exception(
					$"Не удалось проверить коды для разрешительного режима 1260. Code: {response.StatusCode}. {response.ReasonPhrase}");
			}

			var responseBody = await response.Content.ReadAsStreamAsync();
			var result = await JsonSerializer.DeserializeAsync<CodeCheckResponse>(responseBody, _serializeOptions, cancellationToken);

			return result;
		}
	}
}
