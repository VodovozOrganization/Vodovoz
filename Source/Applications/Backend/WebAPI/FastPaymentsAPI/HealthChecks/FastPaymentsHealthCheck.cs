using System;
using System.Net.Http;
using System.Threading.Tasks;
using FastPaymentsAPI.Library.DTO_s;
using FastPaymentsAPI.Library.DTO_s.Requests;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Utils;

namespace FastPaymentsAPI.HealthChecks
{
	public class FastPaymentsHealthCheck : VodovozHealthCheckBase
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public FastPaymentsHealthCheck(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthResult = new VodovozHealthResultDto();

			var fastPaymentControllerResult = await ResponseHelper.GetJsonByUri<OrderDTO>(
				"https://localhost:7290/api/GetOrderId?orderId=3279687",
				_httpClientFactory);

			var fastPaymentControllerIsHealthy = fastPaymentControllerResult != null;

			if(!fastPaymentControllerIsHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("FastPaymentController не прошёл проверку.");
			}

			var paymentStatusControllerResult = await ResponseHelper.GetJsonByUri<FastPaymentStatusDto>(
				"https://localhost:7290/api/GetCheckPaymentStatus?orderId=109654126",
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
