using CashReceiptApi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Settings.CashReceipt;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace CashReceiptApi.HealthChecks
{
	public class CashReceiptApiHealthChecks : VodovozHealthCheckBase
	{
		private readonly IOptions<ServiceOptions> _serviceOptions;
		private readonly ICashReceiptSettings _cashReceiptSettings;

		public CashReceiptApiHealthChecks(
			ILogger<VodovozHealthCheckBase> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<ServiceOptions> serviceOptions,
			ICashReceiptSettings cashReceiptSettings)
			: base(logger, unitOfWorkFactory)
		{
			_serviceOptions = serviceOptions ?? throw new ArgumentNullException(nameof(serviceOptions));
			_cashReceiptSettings = cashReceiptSettings ?? throw new ArgumentNullException(nameof(cashReceiptSettings));
		}

		protected override Task<VodovozHealthResultDto> GetHealthResult()
		{

			try
			{
			}
			catch(Exception ex)
			{
			}

			return Task.FromResult(new VodovozHealthResultDto { IsHealthy = _serviceOptions.Value.HealthCheckCashReceiptId > 0 });
		}
	}
}
