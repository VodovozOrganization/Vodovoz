using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Library.Dto;
using Vodovoz.Domain.Client;

namespace TrueMarkApi.Library
{
	public class TrueMarkApiClient
	{
		private static HttpClient _httpClient;

		public TrueMarkApiClient(string trueMarkApiBaseUrl, string trueMarkApiToken)
		{
			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri(trueMarkApiBaseUrl)
			};
			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", trueMarkApiToken);
		}

		public async Task<TrueMarkResponseResultDto> GetParticipantRegistrationForWaterStatusAsync(string url, string inn, CancellationToken cancellationToken)
		{
			var urlWithParams =  $"{url}?inn={inn}";
			var response = await _httpClient.GetAsync(urlWithParams, cancellationToken);
			var responseBody = await response.Content.ReadAsStreamAsync();
			var responseResult = await JsonSerializer.DeserializeAsync<TrueMarkResponseResultDto>(responseBody, cancellationToken: cancellationToken);

			return responseResult;
		}
	}
}
