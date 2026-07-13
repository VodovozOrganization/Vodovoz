using CustomerAppsApi.Library.V2.Dto.Goods;
using Vodovoz.Core.Domain.Goods;

namespace CustomerAppsApi.Library.V2.Extensions
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
