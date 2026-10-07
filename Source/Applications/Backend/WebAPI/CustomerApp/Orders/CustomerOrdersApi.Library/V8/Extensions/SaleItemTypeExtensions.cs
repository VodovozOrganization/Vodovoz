using CustomerApp.Contracts.Sale;
using CustomerOrders.Abstractions.V8.Sale;
using Vodovoz.Core.Domain.Goods;

namespace CustomerOrdersApi.Library.V8.Extensions
{
	public static class SaleItemTypeExtensions
	{
		public static SaleItemType ToSaleItemType(this NomenclatureCategory source)
		{
			switch(source)
			{
				case NomenclatureCategory.water:
					return SaleItemType.Water;
				case NomenclatureCategory.master:
				case NomenclatureCategory.service:
					return SaleItemType.Service;
				case NomenclatureCategory.equipment:
					return SaleItemType.Equipment;
				default:
					return SaleItemType.Other;
			}
		}
	}
}
