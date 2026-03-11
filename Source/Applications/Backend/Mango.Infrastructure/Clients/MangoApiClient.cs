using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mango.Infrastructure.Clients
{
	public class MangoApiClient : IMangoApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly IOptions<MangoOptions> _mangoOptions;
		private readonly IOptions<SyncOptions> _syncOptions;
		private readonly ILogger<MangoApiClient> _logger;

		public MangoApiClient(
			HttpClient httpClient,
			IOptions<MangoOptions> mangoOptions,
			IOptions<SyncOptions> syncOptions,
			ILogger<MangoApiClient> logger
			)
		{
			_httpClient = httpClient;
			_mangoOptions = mangoOptions;
			_syncOptions = syncOptions;
			_logger = logger;
		}

		public async Task<string> GetCallsRawJsonAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
		{
			var requestJson = JsonSerializer.Serialize(new
            {
                start_date = startDate.ToString("yyyy-MM-dd HH:mm:ss"),
                end_date = endDate.ToString("yyyy-MM-dd HH:mm:ss"),
                limit = _mangoOptions.Value.Limit ,
                offset = 0
            });

            var requestResponse = await PostToMangoAsync(
                "/vpbx/stats/calls/request",
                requestJson,
                cancellationToken);

            var key = ExtractKey(requestResponse);
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException($"Mango did not return key. Response: {requestResponse}");
            }

            for (var attempt = 1; attempt <= _syncOptions.Value.ResultRetryCount; attempt++)
            {
                var resultJson = JsonSerializer.Serialize(new
                {
                    key = key
                });

                var resultResponse = await PostToMangoAsync(
                    "/vpbx/stats/calls/result",
                    resultJson,
                    cancellationToken);

                if (IsCompletedJson(resultResponse))
                {
                    return resultResponse;
                }

                _logger.LogInformation(
                    "Mango result is not ready. Attempt={Attempt}/{RetryCount}",
                    attempt,
                    _syncOptions.Value.ResultRetryCount);

                await Task.Delay(
                    TimeSpan.FromSeconds(_syncOptions.Value.ResultRetryDelaySeconds),
                    cancellationToken);
            }

            throw new TimeoutException("Mango result was not ready in time.");
        }

        private async Task<string> PostToMangoAsync(
            string endpoint,
            string json,
            CancellationToken cancellationToken)
        {
            var sign = CreateSign(_mangoOptions.Value.ApiKey, json, _mangoOptions.Value.ApiSalt);

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["vpbx_api_key"] = _mangoOptions.Value.ApiKey,
                ["sign"] = sign,
                ["json"] = json
            });

            using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Mango request failed. Status={(int)response.StatusCode}, Body={responseBody}");
            }

            return responseBody;
        }

        private static string CreateSign(string apiKey, string json, string apiSalt)
        {
            var raw = apiKey + json + apiSalt;

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

            var sb = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }

        private static string? ExtractKey(string responseBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("key", out var keyElement))
                    return keyElement.GetString();

                if (root.TryGetProperty("result", out var resultElement) &&
                    resultElement.ValueKind == JsonValueKind.Object &&
                    resultElement.TryGetProperty("key", out var nestedKey))
                {
                    return nestedKey.GetString();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsCompletedJson(string response)
        {
            try
            {
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var status))
                {
                    return string.Equals(
                        status.GetString(),
                        "complete",
                        StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
	}
}
