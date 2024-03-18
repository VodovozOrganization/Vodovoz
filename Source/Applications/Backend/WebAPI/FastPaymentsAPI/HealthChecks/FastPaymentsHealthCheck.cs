using System;
using System.Net.Http;
using System.Threading.Tasks;
using FastPaymentsApi.Contracts;
using FastPaymentsApi.Contracts.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace FastPaymentsAPI.HealthChecks
{
	public class FastPaymentsHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public FastPaymentsHealthCheck(ILogger<FastPaymentsHealthCheck> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var orderId = healthSection.GetValue<int>("Variables:OrderId");

			var healthResult = new VodovozHealthResultDto();

			var fastPaymentControllerResult = await ResponseHelper.GetJsonByUri<OrderDTO>(
				$"{baseAddress}/api/GetOrderId?orderId={orderId}",
				_httpClientFactory);

			var fastPaymentControllerIsHealthy = fastPaymentControllerResult != null;

			if(!fastPaymentControllerIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("FastPaymentController не прошёл проверку.");
			}

			var paymentStatusControllerResult = await ResponseHelper.GetJsonByUri<FastPaymentStatusDto>(
				$"{baseAddress}/api/GetCheckPaymentStatus?orderId={orderId}",
				_httpClientFactory);

			var paymentStatusControllerIsHealthy = paymentStatusControllerResult?.PaymentStatus == RequestPaymentStatus.Performed;

			if(!paymentStatusControllerIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("PaymentStatusController не прошёл проверку.");
			}

			healthResult.IsHealthy = fastPaymentControllerIsHealthy && paymentStatusControllerIsHealthy;

			return healthResult;
		}
	}
}
