using BitrixApi.Contracts.Dto;
using BitrixApi.Contracts.Dto.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace BitrixApi.HealthChecks
{
	public class BitrixApiHealthChecks : VodovozHealthCheckBase
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger;
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public BitrixApiHealthChecks(
			ILogger<VodovozHealthCheckBase> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration)
			: base(logger, unitOfWorkFactory)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_httpClient = (httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory)))
				.CreateClient();
			_configuration = configuration;
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			_logger.LogInformation("Проверяем здоровье Bitrix API.");

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

			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
			var responseMessage = await _httpClient.PostAsJsonAsync($"{baseAddress}/api/v1/SendDocumentByEmail", sendReportRequest);

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
