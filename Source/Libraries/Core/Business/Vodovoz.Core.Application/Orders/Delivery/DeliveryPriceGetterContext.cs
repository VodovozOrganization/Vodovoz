using Vodovoz.Core.Domain.Interfaces.Orders;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <inheritdoc/>
	public class DeliveryPriceGetterContext<T> : IDeliveryPriceGetterContext<T>
	{
		/// <inheritdoc/>
		public T Data { get; private set; }

		public static DeliveryPriceGetterContext<T> Create(T data) =>
			new()
			{
				Data = data
			};
	}
}
