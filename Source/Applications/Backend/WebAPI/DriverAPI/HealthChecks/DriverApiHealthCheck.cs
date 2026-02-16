using DriverApi.Contracts.V5;
using DriverApi.Contracts.V5.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;
using VodovozHealthCheck.Providers;

namespace DriverAPI.HealthChecks
{
	public class DriverApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly IConfigurationSection _healthSection;
		private readonly string _baseAddress;
		private string _token;

		public DriverApiHealthCheck(
			ILogger<DriverApiHealthCheck> logger,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHealthCheckServiceInfoProvider serviceInfoProvider)
			: base(logger, serviceInfoProvider, unitOfWorkFactory)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_healthSection = _configuration.GetSection("Health");
			_baseAddress = _healthSection.GetValue<string>("BaseAddress");
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			try
			{
				var tokenResponse  = await GetToken(cancellationToken);

				if(tokenResponse.IsSuccess && tokenResponse.Data?.AccessToken is string token)
				{
					_token =  token;
				}
				else
				{
					return VodovozHealthResultDto.UnhealthyResult($"Не удалось получить токен авторизации: {tokenResponse.ErrorMessage}");
				}
				
				var checks = new[]
				{
					ExecuteHealthCheckSafelyAsync("Проверка получения статуса оплаты заказа посредством QR-кода",
						checkMethodName => CheckGetOrderQRPaymentStatus(checkMethodName, cancellationToken)),
					ExecuteHealthCheckSafelyAsync("Проверка получения МЛ", checkMethodName => CheckGetRouteList(checkMethodName, cancellationToken))
				};

				return await ConcatHealthCheckResultsAsync(checks);
			}
			catch(Exception e)
			{
				return VodovozHealthResultDto.UnhealthyResult(
					$"Не удалось осуществить проверку работоспособности {ServiceInfoProvider.Name}. Ошибка: {e}"
				);
			}
		}

		private async Task<VodovozHealthResultDto> CheckGetRouteList(string checkMethodName, CancellationToken cancellationToken)
		{
			var routeListId = _healthSection.GetValue<string>("Variables:RouteListId");
			
			var result = await HttpResponseHelper.SendRequestAsync<RouteListDto>(
				HttpMethod.Get,
				$"{_baseAddress}/api/v5/GetRouteList?routeListId={routeListId}",
				_httpClientFactory,
				accessToken: _token,
				cancellationToken: cancellationToken);

			var isHealthy = result?.Data?.CompletionStatus == RouteListDtoCompletionStatus.Completed;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<VodovozHealthResultDto> CheckGetOrderQRPaymentStatus(string checkMethodName, CancellationToken cancellationToken)
		{
			var orderId = _healthSection.GetValue<string>("Variables:OrderId");

			var result = await HttpResponseHelper.SendRequestAsync<OrderQrPaymentStatusResponse>(
				HttpMethod.Get,
				$"{_baseAddress}/api/v6/GetOrderQRPaymentStatus?orderId={orderId}",
				_httpClientFactory,
				accessToken: _token,
				cancellationToken: cancellationToken);

			var isHealthy = result?.Data?.QRPaymentStatus == QrPaymentDtoStatus.Paid;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}

		private async Task<HttpResponseWrapper<TokenResponse>> GetToken(CancellationToken cancellationToken)
		{
			var user = _healthSection.GetValue<string>("Authorization:User");
			var password = _healthSection.GetValue<string>("Authorization:Password");
			
			var loginRequestDto = new LoginRequest
			{
				Username = user,
				Password = password
			};

			var tokenResponse = await HttpResponseHelper.SendRequestAsync<TokenResponse>(
				HttpMethod.Post,
				$"{_baseAddress}/api/v6/Authenticate",
				_httpClientFactory,
				loginRequestDto.ToJsonContent(),
				cancellationToken);
			
			return tokenResponse;
		}
	}
}
