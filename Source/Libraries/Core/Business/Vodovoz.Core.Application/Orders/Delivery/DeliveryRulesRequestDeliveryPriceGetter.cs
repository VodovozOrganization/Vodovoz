using System;
using Vodovoz.Core.Domain.Interfaces.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors.Clients;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <inheritdoc/>
	public class DeliveryRulesRequestDeliveryPriceGetter : IDeliveryPriceGetter<DeliveryRulesRequestDeliveryPriceGetterContext>
	{
		private readonly CustomerCartWaterCounts _cartWaterCounts;
		private readonly int _paidDeliveryId;

		public DeliveryRulesRequestDeliveryPriceGetter(
			INomenclatureSettings nomenclatureSettings,
			CustomerCartWaterCounts cartWaterCounts)
		{
			_cartWaterCounts = cartWaterCounts ?? throw new ArgumentNullException(nameof(cartWaterCounts));
			_paidDeliveryId =
				(nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings)))
				.PaidDeliveryNomenclatureId;
		}
		
		/// <inheritdoc/>
		public Result<decimal> GetDeliveryPrice(IDeliveryPriceGetterContext<DeliveryRulesRequestDeliveryPriceGetterContext> context)
		{
			var data = context.Data;
			if(data.District is null)
			{
				return Result.Failure<decimal>(DeliveryPointErrors.CouldNotCalculateDeliveryBecauseDistrictNotFound());
			}

			_cartWaterCounts.Initialize(data.CartItems);
			var price = data.District.GetDeliveryPrice(data.WeekDay, _cartWaterCounts, 0m);

			return Result.Success(price);
		}
	}
}
