using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;

namespace VodovozBusiness.EntityRepositories.Edo
{
	/// <summary>
	/// Данные по отсканированному водителем коду ЧЗ
	/// </summary>
	public class DriversScannedCodeDataNode
	{
		/// <summary>
		/// Отсканированный код
		/// </summary>
		public DriversScannedTrueMarkCode DriversScannedCode { get; set; }
		/// <summary>
		/// Заказ
		/// </summary>
		public OrderEntity Order { get; set; }
		/// <summary>
		/// Строка заказа
		/// </summary>
		public OrderItemEntity OrderItem { get; set; }
		/// <summary>
		/// Адрес маршрутного листа
		/// </summary>
		public RouteListItemEntity RouteListAddress { get; set; }
	}
}
