using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <summary>
	/// Данные для расчета стоимости доставки в заказе
	/// </summary>
	public class OrderDeliveryPriceContext
	{
		/// <summary>
		/// Unit of work
		/// </summary>
		public IUnitOfWork UnitOfWork { get; private set; }
		/// <summary>
		/// Заказ
		/// </summary>
		public Order Order  { get; private set; }

		public static OrderDeliveryPriceContext Create(IUnitOfWork unitOfWork, Order order) =>
			new OrderDeliveryPriceContext
			{
				UnitOfWork = unitOfWork,
				Order = order
			};
	}
}
