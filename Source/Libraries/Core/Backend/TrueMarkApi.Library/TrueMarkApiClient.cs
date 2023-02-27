using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Library.Dto;

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

		public async Task<ProductInstancesInfo> GetProductInstanceInfoAsync(IEnumerable<string> identificationCodes, CancellationToken cancellationToken)
		{
			string content = JsonSerializer.Serialize(identificationCodes.ToArray());
			HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync("RequestProductInstanceInfo", httpContent, cancellationToken);
			var responseBody = await response.Content.ReadAsStreamAsync();
			var responseResult = await JsonSerializer.DeserializeAsync<ProductInstancesInfo>(responseBody, cancellationToken: cancellationToken);

			return responseResult;
		}
		
		public async Task<IList<ParticipantRegistrationDto>> GetParticipantsRegistrations(string url, IList<string> notRegisteredInns,
			CancellationToken cancellationToken)
		{
			var serializedNotRegisteredInns = JsonSerializer.Serialize(notRegisteredInns);
			var content = new StringContent(serializedNotRegisteredInns, Encoding.UTF8, "application/json");
			var response = await _httpClient.PostAsync(url, content, cancellationToken);

			if(response.IsSuccessStatusCode)
			{
				var responseBody = await response.Content.ReadAsStreamAsync();

				var registrations = await JsonSerializer.DeserializeAsync<IList<ParticipantRegistrationDto>>(responseBody, cancellationToken: cancellationToken);

				return registrations;
			}

			return null;
		}
	}
}
