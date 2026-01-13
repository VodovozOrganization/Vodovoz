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

namespace CustomerAppsApi.HealthChecks
{
	public partial class CustomerAppsApiHealthCheck : VodovozHealthCheckBase
	{
		private const string _serviceName = "Сервис регистрации/авторизации пользователей";
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly IConfigurationSection _healthSection;
		private readonly string _baseAddress;

		public CustomerAppsApiHealthCheck(ILogger<VodovozHealthCheckBase> logger,
			IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IBusControl busControl)
			: base(logger, unitOfWorkFactory, busControl)
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

				return await ConcatHealthCheckResultsAsync(checks, _serviceName);
			}
			catch(Exception e)
			{
				return VodovozHealthResultDto.UnhealthyResult(
					$"Не удалось осуществить проверку работоспособности сервиса: {_serviceName}. Ошибка: {e}"
				);
			}
		}
	}
}
