using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Contracts.Requests;
using Telegram.Contracts.Response;
using Telegram.GatewayApi.Client.Configs;

namespace Telegram.GatewayApi.Client
{
	public class TelegramGatewayApiClient : ITelegramGatewayApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly TelegramGatewayApiOptions _gatewayApiOptions;

		public TelegramGatewayApiClient(
			HttpClient httpClient,
			IOptionsSnapshot<TelegramGatewayApiOptions> gatewayApiOptions)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_gatewayApiOptions = (gatewayApiOptions ?? throw new ArgumentNullException(nameof(gatewayApiOptions))).Value;
		}

		public async Task<ResponseDto> SendVerificationMessage(SendVerificationMessageRequest dto)
		{
			var response = await _httpClient.PostAsJsonAsync(_gatewayApiOptions.SendVerificationMessageEndpoint, dto);
			return await response.Content.ReadFromJsonAsync<ResponseDto>();
		}
		
		public async Task<ResponseDto> CheckSendAbility(CheckSendAbilityRequest dto)
		{
			var response = await _httpClient.PostAsJsonAsync(_gatewayApiOptions.CheckSendAbilityEndpoint, dto);
			return await response.Content.ReadFromJsonAsync<ResponseDto>();
		}
		
		public async Task<ResponseDto> CheckVerificationStatus(CheckVerificationStatusRequest dto)
		{
			var response = await _httpClient.PostAsJsonAsync(_gatewayApiOptions.CheckVerificationStatusEndpoint, dto);
			return await response.Content.ReadFromJsonAsync<ResponseDto>();
		}
	}
}
