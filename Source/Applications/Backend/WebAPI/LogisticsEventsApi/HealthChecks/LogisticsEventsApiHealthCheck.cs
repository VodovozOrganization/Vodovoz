using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;
using VodovozHealthCheck.Providers;

namespace LogisticsEventsApi.HealthChecks
{
	public class LogisticsEventsApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public LogisticsEventsApiHealthCheck(
			ILogger<LogisticsEventsApiHealthCheck> logger,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHealthCheckServiceInfoProvider serviceInfoProvider) : base(logger, serviceInfoProvider, unitOfWorkFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			var healthSection = _configuration.GetSection("Health");

			var baseAddress = healthSection.GetValue<string>("BaseAddress");

			var user = healthSection.GetValue<string>("Authorization:User");
			var password = healthSection.GetValue<string>("Authorization:Password");

			var healthResult = new VodovozHealthResultDto();

			var loginRequestDto = new LoginRequest
			{
				Username = user,
				Password = password
			};

			var tokenResponse = await HttpResponseHelper.SendRequestAsync<TokenResponse>(
				HttpMethod.Post,
				$"{baseAddress}/api/Authenticate",
				_httpClientFactory,
				loginRequestDto.ToJsonContent(),
				cancellationToken);
			
			var todayCompletedEventsResult = await HttpResponseHelper.SendRequestAsync<HttpResponseMessage>(
				HttpMethod.Get,
				$"{baseAddress}/api/GetTodayCompletedEvents",
				_httpClientFactory,
				accessToken: tokenResponse.Data?.AccessToken,
				cancellationToken: cancellationToken);

			var todayCompletedEventsIsHealthy = todayCompletedEventsResult?.Data?.StatusCode == HttpStatusCode.OK;

			if(!todayCompletedEventsIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("GetTodayCompletedEvents не прошёл проверку.");
			}

			healthResult.IsHealthy = todayCompletedEventsIsHealthy;

			return healthResult;
		}
	}
}
