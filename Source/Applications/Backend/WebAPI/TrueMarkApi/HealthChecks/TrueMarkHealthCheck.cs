using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Options;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace TrueMarkApi.HealthChecks
{
	public class TrueMarkHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IOptions<TrueMarkApiOptions> _options;

		public TrueMarkHealthCheck(
			ILogger<TrueMarkHealthCheck> logger,
			IHttpClientFactory httpClientFactory,
			IOptions<TrueMarkApiOptions> options)
			: base(logger)
		{
			_httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
			_options = options ?? throw new System.ArgumentNullException(nameof(options));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthResult = new VodovozHealthResultDto();

			var controllerIsHealthy = await CheckControllerIsHealthyAsync();

			if(!controllerIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("Не пройдена проверка контроллера.");
			}

			healthResult.IsHealthy = controllerIsHealthy;

			return healthResult;
		}

		private async Task<bool> CheckControllerIsHealthyAsync()
		{
			var httpClient = _httpClientFactory.CreateClient();
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.Value.InternalSecurityKey);
			var urlWithParams = $"http://localhost:5000/api/ParticipantRegistrationForWater?inn={7816453294}";
			var response = await httpClient.GetAsync(urlWithParams);
			var responseBody = await response.Content.ReadAsStreamAsync();
			var responseResult = await JsonSerializer.DeserializeAsync<TrueMarkRegistrationResultDto>(responseBody);

			if(responseResult != null && responseResult.RegistrationStatusString == "Зарегистрирован")
			{
				return true;
			}

			return false;
		}
	}
}
