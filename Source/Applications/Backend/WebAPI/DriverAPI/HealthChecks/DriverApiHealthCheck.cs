using DriverAPI.DTOs.V4;
using DriverAPI.Library.DTOs;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Utils;

namespace DriverAPI.HealthChecks
{
	public class DriverApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public DriverApiHealthCheck(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthResult = new VodovozHealthResultDto();

			var loginRequestDto = new LoginRequestDto
			{
				Username = "rab",
				Password = "123"
			};

			var tokenResponse = await ResponseHelper.PostJsonByUri<LoginRequestDto, TokenResponseDto>(
				"https://localhost:5001/api/v4/Authenticate",
				_httpClientFactory,
				loginRequestDto);

			var orderQrPaymentStatus = await ResponseHelper.GetJsonByUri<OrderQRPaymentStatusResponseDto>(
				"https://localhost:5001/api/v4/GetOrderQRPaymentStatus?orderId=3282267",
				_httpClientFactory,
				tokenResponse.AccessToken);

			var orderQrPaymentStatusIsHealthy = orderQrPaymentStatus.QRPaymentStatus == QRPaymentDTOStatus.Paid;

			if(!orderQrPaymentStatusIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("GetOrderQRPaymentStatus не прошёл проверку.");
			}

			var routeList = await ResponseHelper.GetJsonByUri<RouteListDto>(
				"https://localhost:5001/api/v4/GetRouteList?routeListId=288409",
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
