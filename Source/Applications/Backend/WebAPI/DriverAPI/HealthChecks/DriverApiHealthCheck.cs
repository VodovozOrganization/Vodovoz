using System;
using System.Net.Http;
using System.Threading.Tasks;
using DriverApi.Contracts.V5;
using DriverApi.Contracts.V5.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace DriverAPI.HealthChecks
{
	public class DriverApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public DriverApiHealthCheck(ILogger<DriverApiHealthCheck> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");

			var baseAddress = healthSection.GetValue<string>("BaseAddress");

			var user = healthSection.GetValue<string>("Authorization:User");
			var password = healthSection.GetValue<string>("Authorization:Password");

			var orderId = healthSection.GetValue<string>("Variables:OrderId");
			var routeListId = healthSection.GetValue<string>("Variables:RouteListId");

			var healthResult = new VodovozHealthResultDto();

			var loginRequestDto = new LoginRequest
			{
				Username = user,
				Password = password
			};

			var tokenResponse = await ResponseHelper.PostJsonByUri<LoginRequest, TokenResponse>(
				$"{baseAddress}/api/v5/Authenticate",
				_httpClientFactory,
				loginRequestDto);

			var orderQrPaymentStatus = await ResponseHelper.GetJsonByUri<OrderQrPaymentStatusResponse>(
				$"{baseAddress}/api/v5/GetOrderQRPaymentStatus?orderId={orderId}",
				_httpClientFactory,
				tokenResponse.AccessToken);

			var orderQrPaymentStatusIsHealthy = orderQrPaymentStatus.QRPaymentStatus == QrPaymentDtoStatus.Paid;

			if(!orderQrPaymentStatusIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("GetOrderQRPaymentStatus не прошёл проверку.");
			}

			var routeList = await ResponseHelper.GetJsonByUri<RouteListDto>(
				$"{baseAddress}/api/v5/GetRouteList?routeListId={routeListId}",
				_httpClientFactory,
				tokenResponse.AccessToken);

			var getRouteListIsHealthy = routeList != null;

			if(!getRouteListIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("GetRouteList не прошёл проверку.");
			}

			healthResult.IsHealthy = orderQrPaymentStatusIsHealthy && getRouteListIsHealthy;

			return healthResult;
		}
	}
}
