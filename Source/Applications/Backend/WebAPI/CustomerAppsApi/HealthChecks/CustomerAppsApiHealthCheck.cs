using CustomerAppsApi.Library.Dto.Counterparties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.HealthChecks
{
	public class CustomerAppsApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public CustomerAppsApiHealthCheck(ILogger<CustomerAppsApiHealthCheck> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");

			var cameFromId = healthSection.GetValue<int>("Variables:CameFromId");
			var externalCounterpartyId = healthSection.GetValue<string>("Variables:ExternalCounterpartyId");
			var phoneNumber = healthSection.GetValue<string>("Variables:PhoneNumber");

			var request = new CounterpartyContactInfoDto
			{
				CameFromId = cameFromId,
				ExternalCounterpartyId = new Guid(externalCounterpartyId),
				PhoneNumber = phoneNumber
			};

			var response = await ResponseHelper.PostJsonByUri<CounterpartyContactInfoDto, CounterpartyIdentificationDto>(
				$"{baseAddress}/api/GetCounterparty",
				_httpClientFactory,
				request);

			var isHealthy = response?.CounterpartyIdentificationStatus == CounterpartyIdentificationStatus.Success;

			return new() { IsHealthy = isHealthy };
		}
	}
}
