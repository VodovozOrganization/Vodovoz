using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Tools.Orders
{
	/// <summary>
	/// Состояние заказа из корзины для подбора правил доставки
	/// </summary>
	public class OnlineOrderFromCartStateKey : ComparerDeliveryPrice
	{
		/// <summary>
		/// Иницализация класса
		/// </summary>
		/// <param name="onlineOrder">Данные заказа</param>
		public virtual void InitializeFields(IOnlineOrderFromCart onlineOrder)
		{
			Initialize(onlineOrder.Goods, onlineOrder.DeliveryDate);
		}
	}
}
