using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Extensions
{
	public static class NomenclatureCategoryExtensions
	{
		public static SaleItemType ToSaleItemType(this NomenclatureCategory category)
		{
			switch(category)
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
