using System;
using System.Net.Http;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Requests;
using Microsoft.Extensions.Configuration;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Utils;

namespace FastPaymentsAPI.HealthChecks
{
	public class FastPaymentsHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public FastPaymentsHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var healthResult = new VodovozHealthResultDto();

			var fastPaymentControllerResult = await ResponseHelper.GetJsonByUri<OrderDTO>(
				$"{baseAddress}/api/GetOrderId?orderId=3279687",
				_httpClientFactory);

			var fastPaymentControllerIsHealthy = fastPaymentControllerResult != null;

			if(!fastPaymentControllerIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("FastPaymentController не прошёл проверку.");
			}

			var paymentStatusControllerResult = await ResponseHelper.GetJsonByUri<FastPaymentStatusDto>(
				$"{baseAddress}/api/GetCheckPaymentStatus?orderId=109654126",
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
