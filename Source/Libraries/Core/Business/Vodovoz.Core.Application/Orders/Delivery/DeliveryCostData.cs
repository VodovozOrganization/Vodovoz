using VodovozBusiness.Domain.Orders.Delivery;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	public class DeliveryCostData : IDeliveryCostData
	{
		/// <inheritdoc/>
		public decimal? DeliveryPrice { get; set; }
		/// <inheritdoc/>
		public string Message { get; set; }
		/// <inheritdoc/>
		public IMaxVolumeTotalBottles MaxVolumeTotalBottles { get; set; }
	}
}
