using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;
using VodovozHealthCheck.Providers;

namespace MailganerEventsDistributorApi.HealthChecks
{
	public class MailjetEventsDistributeHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;

		public MailjetEventsDistributeHealthCheck(
			ILogger<MailjetEventsDistributeHealthCheck> logger,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHttpClientFactory httpClientFactory,
			IHealthCheckServiceInfoProvider serviceInfoProvider)
		: base(logger, serviceInfoProvider, unitOfWorkFactory)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");

			var response = await HttpResponseHelper.CheckUriExistsAsync($"{baseAddress}/Test", _httpClientFactory);
			
			return VodovozHealthResultDto.FromCondition("Сервис почты Mailganer", response.IsSuccess, response.ErrorMessage);
		}
	}
}
