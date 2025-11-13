using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="NonReturnReason"/>
	/// </summary>
	public static class NonReturnReasonExtensions
	{
		/// <summary>
		/// Маппинг причины не забора имущества в <see cref="TareNonReturnReasonDto"/>
		/// </summary>
		/// <param name="nonReturnReason"></param>
		/// <returns></returns>
		public static TareNonReturnReasonDto MapToTareNonReturnReasonDtoV1(this NonReturnReason nonReturnReason)
			=> new TareNonReturnReasonDto
			{
				Id = nonReturnReason.Id,
				Name = nonReturnReason.Name,
				NeedForfeit = nonReturnReason.NeedForfeit,
			};

		/// <summary>
		/// Маппинг причин не забора имущества в IEnumerable <see cref="DeliveryIntervalDto"/>
		/// </summary>
		/// <param name="nonReturnReasons"></param>
		/// <returns></returns>
		public static IEnumerable<TareNonReturnReasonDto> MapToTareNonReturnReasonDtoV1(this IEnumerable<NonReturnReason> nonReturnReasons)
			=> nonReturnReasons.Select(MapToTareNonReturnReasonDtoV1);
	}
}
