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

namespace FastPaymentsAPI.HealthChecks
{
	public partial class FastPaymentsHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly IConfigurationSection _healthSection;
		private readonly string _baseAddress;
		private const string _serviceName = "Сервис СБП Авангарда, для оплаты по QR.";

		public FastPaymentsHealthCheck(ILogger<FastPaymentsHealthCheck> logger,
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
					CheckFastPaymentsController(cancellationToken),
					CheckPaymentStatusController(cancellationToken)
				};

				return await ConcatHealthCheckResultsAsync(checks, _serviceName);
			}
			catch(Exception e)
			{
				return VodovozHealthResultDto.UnhealthyResult(
					$"Не удалось осуществить проверку работоспособности сервиса регистрации онлайн заказов и заявок на звонок. Ошибка: {e}"
				);
			}
		}
	}
}
