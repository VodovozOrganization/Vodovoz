using System.Collections.Generic;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Информация для разделения заказа по организациям, в зависимости от наполнения
	/// </summary>
	public class PartitionedOrderByOrganizations
	{
		/// <summary>
		/// Можно разделять заказ с залогами
		/// <c>true</c> можно разделять и в заказе есть или нет залогов.
		/// <c>false</c> нельзя разделять. Сумма залогов превышает сумму в любой из частей заказа
		/// </summary>
		public bool CanSplitOrderWithDeposits { get; set; }
		/// <summary>
		/// Части заказа
		/// </summary>
		public IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> OrderParts { get; set; }
	}
}
