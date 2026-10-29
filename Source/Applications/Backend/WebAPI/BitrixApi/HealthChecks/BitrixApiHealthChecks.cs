using BitrixApi.Contracts.Dto;
using BitrixApi.Contracts.Dto.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;
using VodovozHealthCheck.Providers;

namespace BitrixApi.HealthChecks
{
	public class BitrixApiHealthChecks : VodovozHealthCheckBase
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public BitrixApiHealthChecks(
			ILogger<VodovozHealthCheckBase> logger,
			IHttpContextAccessor httpContextAccessor,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			IHealthCheckServiceInfoProvider serviceInfoProvider)
			: base(logger, serviceInfoProvider, httpContextAccessor, unitOfWorkFactory)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration;
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Проверяем работоспособность Bitrix API.");

			var healthSection = _configuration.GetSection("Health");

			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var apiKey = healthSection.GetValue<string>("Authorization:ApiKey");

			var sendReportRequest = new SendReportRequest
			{
				CounterpartyInn = "0000000000",
				OrganizationId = 111111,
				EmailAdress = "super_mega_test_email@vodovoz-spb.ru",
				ReportType = ReportTypeDto.ReconciliationStatement
			};

			var responseMessage = await HttpResponseHelper.SendRequestAsync<HttpResponseMessage>(
				HttpMethod.Post,
				$"{baseAddress}/api/v1/SendDocumentByEmail",
				_httpClientFactory,
				sendReportRequest.ToJsonContent(),
				cancellationToken,
				apiKey: "Authorization",
				apiKeyValue: $"Bearer {apiKey}");

			var healthResult = new VodovozHealthResultDto
			{
				IsHealthy = true
			};

			if(responseMessage is null
				|| responseMessage.StatusCode != HttpStatusCode.NotFound)
			{
				healthResult.IsHealthy = false;
				healthResult.AdditionalUnhealthyResults.Add("Тест эндпоинта CounterpartyDocuments не прошел проверку");

				return healthResult;
			}

			return healthResult;
		}
	}
}
