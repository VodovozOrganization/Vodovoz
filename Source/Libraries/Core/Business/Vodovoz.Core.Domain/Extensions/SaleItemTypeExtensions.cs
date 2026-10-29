using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Extensions
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
