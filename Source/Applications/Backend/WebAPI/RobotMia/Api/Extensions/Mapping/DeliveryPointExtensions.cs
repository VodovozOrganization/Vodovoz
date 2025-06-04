using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.RobotMia.Contracts.Responses.V1;
using VodovozBusiness.Extensions;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="DeliveryPoint"/>
	/// </summary>
	public static class DeliveryPointExtensions
	{
		/// <summary>
		/// Маппинг точки доставки в <see cref="DeliveryPointDto"/>
		/// </summary>
		/// <returns></returns>
		public static DeliveryPointDto MapToDeliveryPointDtoV1(this DeliveryPoint deliveryPoint)
			=> new DeliveryPointDto
			{
				Id = deliveryPoint.Id,
				Address = deliveryPoint.GetAddressString(),
			};

		/// <summary>
		/// Маппинг точки доставки в IEnumerable <see cref="DeliveryPointDto"/>
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<DeliveryPointDto> MapToDeliveryPointDtoV1(this IEnumerable<DeliveryPoint> deliveryPoints)
			=> deliveryPoints.Select(MapToDeliveryPointDtoV1);
	}
}
