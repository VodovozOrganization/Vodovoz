using System.Collections.Generic;
using CustomerAppsApi.Library.V2.Dto.Goods;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface ISaleItemFactory
	{
		NomenclaturesPricesAndStockDto CreateNomenclaturesPricesAndStockDto(NomenclatureOnlineParametersData parametersData);
		SaleItemsDto CreateSaleItemsDto(IEnumerable<OnlineNomenclatureNode> onlineNomenclatures);
		object CreateWaterSaleItem(OnlineNomenclatureNode nomenclatureNode);
		object CreateServiceSaleItem(OnlineNomenclatureNode nomenclatureNode);
		object CreateEquipmentSaleItem(OnlineNomenclatureNode nomenclatureNode);
		object CreateOtherSaleItem(OnlineNomenclatureNode nomenclatureNode);
		IEnumerable<object> CreatePromoSetSaleItems(PromotionalSetOnlineParametersData parametersData);
		object CreateFreeRentPackageSaleItems(FreeRentPackageWithOnlineParametersNode packageNode);
	}
}
