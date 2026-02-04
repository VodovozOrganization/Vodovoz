using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.RobotMia.Contracts.Responses.V1;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace Vodovoz.RobotMia.Api.HealthCheck
{
	public partial class RobotMiaApiHealthCheck
	{
		private async Task<VodovozHealthResultDto> CheckBottlesForReturnFromDeliveryPointController(CancellationToken cancellationToken)
		{
			var chekcs = new[]
			{
				ExecuteHealthCheckSafelyAsync("Запрос количества бутылей, ожидаемых к возврату с адреса",
					checkMethodName => BottlesForReturnFromDeliveryPoint(checkMethodName, cancellationToken)),
			};

			return await ConcatHealthCheckResultsAsync(chekcs);

		}

		private async Task<VodovozHealthResultDto> BottlesForReturnFromDeliveryPoint(string checkMethodName, CancellationToken cancellationToken)
		{
			var getBottlesForReturnFromDeliveryPointSection = _healthSection.GetSection("GetBottlesForReturnFromDeliveryPoint");

			var callId = getBottlesForReturnFromDeliveryPointSection.GetValue<Guid>("call_id");
			var deliveryPointId = getBottlesForReturnFromDeliveryPointSection.GetValue<int>("delivery_point_id");

			var result = await HttpResponseHelper.SendRequestAsync<BottlesForReturnFromDeliveryPointResponse>(
				HttpMethod.Get,
				$"{_baseAddress}/api/v1/BottlesForReturnFromDeliveryPoint?call_id={callId}&delivery_point_id={deliveryPointId}",
				_httpClientFactory,
				accessToken: "",
				cancellationToken: cancellationToken);

			var isHealthy = result.Data?.BottlesAtDeliveryPoint > 0;

			return VodovozHealthResultDto.FromCondition(checkMethodName, isHealthy, result.ErrorMessage);
		}
	}
}
