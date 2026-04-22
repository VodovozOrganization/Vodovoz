using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mango.Business.Interfaces;
using Mango.Contracts.V1.Options;
using Mango.Contracts.V1.Request;
using Mango.Contracts.V1.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mango.Infrastructure.Clients
{
	public class MangoApiClient : IMangoApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly IOptions<MangoOptions> _mangoOptions;
		private readonly IOptions<SyncOptions> _syncOptions;
		private readonly IOptions<JsonSerializerOptions> _jsonOptions;
		private readonly ILogger<MangoApiClient> _logger;
		
		public MangoApiClient(
			HttpClient httpClient,
			IOptions<MangoOptions> mangoOptions,
			IOptions<SyncOptions> syncOptions,
			IOptions<JsonSerializerOptions> jsonOptions,
			ILogger<MangoApiClient> logger
			)
		{
			_httpClient = httpClient ?? throw new  ArgumentNullException(nameof(httpClient));
			_mangoOptions = mangoOptions ?? throw new  ArgumentNullException(nameof(mangoOptions));
			_syncOptions = syncOptions ?? throw new  ArgumentNullException(nameof(syncOptions));
			_jsonOptions = jsonOptions ?? throw new  ArgumentNullException(nameof(jsonOptions));
			_logger = logger ?? throw new  ArgumentNullException(nameof(logger));
		}

		public async Task<GroupsResponse> GetGroupsAsync(CancellationToken cancellationToken)
        {
            var request = new GroupsRequest();

            var json = JsonSerializer.Serialize(request, _jsonOptions.Value);
            var sign = CreateSign(_mangoOptions.Value.ApiKey, json, _mangoOptions.Value.ApiSalt);

            var response = await PostFormAsync<GroupsResponse>(
	            _mangoOptions.Value.GroupsUrl,
	            _mangoOptions.Value.ApiKey,
                sign,
                json,
                cancellationToken);

            return response;
        }

		public async Task<CallsResponse> GetCallsAsync(
			string key,
			CancellationToken cancellationToken)
		{
			var request = new CallsRequest
			{
				Key = key
			};

			var json = JsonSerializer.Serialize(request, _jsonOptions.Value);
			var sign = CreateSign(_mangoOptions.Value.ApiKey, json, _mangoOptions.Value.ApiSalt);

			var response = await PostFormAsync<CallsResponse>(
				_mangoOptions.Value.CallsResult,
				_mangoOptions.Value.ApiKey,
				sign,
				json,
				cancellationToken);
			
			return response;
		}

		public async Task<CallsStatResponse> GetCallsStatAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
        {
	        var request = new CallsStatRequest()
	        {
		        StartDate = fromDate.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
		        EndDate = toDate.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
		        Limit = _mangoOptions.Value.Limit.ToString(CultureInfo.InvariantCulture),
		        Offset = _mangoOptions.Value.Offset.ToString(CultureInfo.InvariantCulture),
	        };

	        var json = JsonSerializer.Serialize(request, _jsonOptions.Value);
	        var sign = CreateSign(_mangoOptions.Value.ApiKey, json, _mangoOptions.Value.ApiSalt);
	        
	        var response = await PostFormAsync<CallsStatResponse>(
		        _mangoOptions.Value.CallsUrl,
		        _mangoOptions.Value.ApiKey,
		        sign,
		        json,
		        cancellationToken);

	        return response;

        }

        private async Task<T> PostFormAsync<T>(
            string url,
            string apiKey,
            string sign,
            string json,
            CancellationToken cancellationToken)
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["vpbx_api_key"] = apiKey,
                ["sign"] = sign,
                ["json"] = json
            });

            using var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation(
                "Mango response received. Url={Url}, StatusCode={StatusCode}",
                url,
                (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Mango request failed. Url={Url}, StatusCode={StatusCode}, Response={Response}",
                    url,
                    (int)response.StatusCode,
                    raw);

                response.EnsureSuccessStatusCode();
            }

            var result = JsonSerializer.Deserialize<T>(raw, _jsonOptions.Value);

            if (result is null)
            {
                _logger.LogError(
                    "Failed to deserialize Mango response. Url={Url}, Response={Response}",
                    url,
                    raw);

                throw new InvalidOperationException(
                    $"Failed to deserialize Mango response from {url}");
            }

            return result;
        }

        private static string CreateSign(string apiKey, string json, string apiSalt)
        {
            var raw = apiKey + json + apiSalt;

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
	}
}
