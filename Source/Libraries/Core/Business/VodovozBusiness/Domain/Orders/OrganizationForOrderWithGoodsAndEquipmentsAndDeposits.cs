using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Orders
{
	/// <summary>
	/// Организация с товарами, оборудованием, залогами. Для возможного разбиения заказа на несколько
	/// </summary>
	public class OrganizationForOrderWithGoodsAndEquipmentsAndDeposits
	{
		public OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(
			Organization organization,
			IEnumerable<OrderItem> orderItems = null,
			IEnumerable<OrderEquipment> orderEquipments = null)
		{
			Organization = organization;
			OrderItems = orderItems;
			OrderEquipments = orderEquipments;
		}
		
		/// <summary>
		/// Организация
		/// </summary>
		public Organization Organization { get; }
		/// <summary>
		/// Товары
		/// </summary>
		public IEnumerable<OrderItem> OrderItems { get; }
		/// <summary>
		/// Оборудование
		/// </summary>
		public IEnumerable<OrderEquipment> OrderEquipments { get; }
		/// <summary>
		/// Залоги
		/// </summary>
		public IEnumerable<OrderDepositItem> OrderDepositItems { get; set; }
		/// <summary>
		/// Сумма товаров
		/// </summary>
		public decimal GoodsSum => OrderItems.Sum(x => x.Sum);
	}

	public class OrderForOrderWithGoodsEquipmentsAndDeposits
	{
		/// <summary>
		/// Можно разделять заказ с залогами
		/// <c>true</c> можно разделять и в заказе есть или нет залогов.
		/// <c>false</c> нельзя разделять. Сумма залогов превышает сумму в любой из частей заказа
		/// </summary>
		public bool CanSplitOrderWithDeposits { get; set; }
		public IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> OrderParts { get; set; }
	}
}
