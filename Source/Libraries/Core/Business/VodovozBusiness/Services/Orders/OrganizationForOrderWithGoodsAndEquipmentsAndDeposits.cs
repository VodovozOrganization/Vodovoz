using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Организация с товарами, оборудованием, залогами. Часть общего заказа при разбиении
	/// </summary>
	public class OrganizationForOrderWithGoodsAndEquipmentsAndDeposits
	{
		public OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(
			Organization organization,
			IEnumerable<IProduct> goods = null,
			IEnumerable<OrderEquipment> orderEquipments = null)
		{
			Organization = organization;
			Goods = goods;
			OrderEquipments = orderEquipments;
		}
		
		/// <summary>
		/// Организация
		/// </summary>
		public Organization Organization { get; }
		/// <summary>
		/// Товары
		/// </summary>
		public IEnumerable<IProduct> Goods { get; }
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
		public decimal GoodsSum => Goods.Sum(x => x.ActualSum);
	}
}
