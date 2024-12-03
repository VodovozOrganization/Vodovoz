using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace WarehouseApi.HealthChecks
{
	public class WarehouseApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public WarehouseApiHealthCheck(
			ILogger<WarehouseApiHealthCheck> logger,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory) : base(logger, unitOfWorkFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");

			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var orderId = healthSection.GetValue<string>("OrderId");

			var user = healthSection.GetValue<string>("Authorization:User");
			var password = healthSection.GetValue<string>("Authorization:Password");

			var healthResult = new VodovozHealthResultDto();

			var loginRequestDto = new LoginRequest
			{
				Username = user,
				Password = password
			};

			var tokenResponse = await ResponseHelper.PostJsonByUri<LoginRequest, TokenResponse>(
				$"{baseAddress}/api/Authenticate",
				_httpClientFactory,
				loginRequestDto);

			var orderData = await ResponseHelper.GetByUri(
				$"{baseAddress}/api/GetOrder?orderId={orderId}",
				_httpClientFactory,
				tokenResponse.AccessToken);

			var isHealthy = ((HttpResponseMessage)orderData).StatusCode == HttpStatusCode.OK;

			if(!isHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("GetOrder не прошёл проверку.");
			}

			healthResult.IsHealthy = isHealthy;

			return healthResult;
		}
	}
}
