using DriverAPI.DTOs.V4;
using DriverAPI.Library.DTOs;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
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

			var loginRequestDto = new LoginRequestDto
			{
				Username = user,
				Password = password
			};

			var tokenResponse = await ResponseHelper.PostJsonByUri<LoginRequestDto, TokenResponseDto>(
				$"{baseAddress}/api/v4/Authenticate",
				_httpClientFactory,
				loginRequestDto);

			var orderQrPaymentStatus = await ResponseHelper.GetJsonByUri<OrderQRPaymentStatusResponseDto>(
				$"{baseAddress}/api/v4/GetOrderQRPaymentStatus?orderId={orderId}",
				_httpClientFactory,
				tokenResponse.AccessToken);

			var orderQrPaymentStatusIsHealthy = orderQrPaymentStatus.QRPaymentStatus == QRPaymentDTOStatus.Paid;

			if(!orderQrPaymentStatusIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("GetOrderQRPaymentStatus не прошёл проверку.");
			}

			var routeList = await ResponseHelper.GetJsonByUri<RouteListDto>(
				$"{baseAddress}/api/v4/GetRouteList?routeListId={routeListId}",
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
