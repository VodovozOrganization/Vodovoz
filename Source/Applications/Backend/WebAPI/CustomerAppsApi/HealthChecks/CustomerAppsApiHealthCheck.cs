using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Providers;

namespace CustomerAppsApi.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly IConfigurationSection _healthSection;
		private readonly string _baseAddress;

		public CustomerAppsApiHealthCheck(ILogger<VodovozHealthCheckBase> logger,
			IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IBusControl busControl,
			IHealthCheckServiceInfoProvider serviceInfoProvider)
			: base(logger, serviceInfoProvider, unitOfWorkFactory, busControl)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_healthSection = _configuration.GetSection("Health");
			_baseAddress = _healthSection.GetValue<string>("BaseAddress");
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			try
			{
				var checks = new[]
					{
						CheckCounterpartyController(cancellationToken),
						CheckCounterpartyBottlesDebtController(cancellationToken),
						CheckDeliveryPointController(cancellationToken),
						CheckNomenclatureController(cancellationToken),
						CheckOrdersController(cancellationToken),
						CheckPromotionalSetController(cancellationToken),
						CheckRentPackagesController(cancellationToken),
						CheckSendingController(cancellationToken),
						CheckWarehouseController(cancellationToken)
					};

				return await ConcatHealthCheckResultsAsync(checks);
			}
			catch(Exception e)
			{
				return VodovozHealthResultDto.UnhealthyResult(
					$"Не удалось осуществить проверку работоспособности сервиса: {ServiceInfoProvider.Name}. Ошибка: {e}"
				);
			}
		}
	}
}
