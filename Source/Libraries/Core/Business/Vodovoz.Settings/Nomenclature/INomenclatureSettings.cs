namespace Vodovoz.Settings.Nomenclature
{
	public interface INomenclatureSettings
	{
		int[] EquipmentKindsHavingGlassHolder { get; }
		int[] EquipmentForCheckProductGroupsIds { get; }
		int ReturnedBottleNomenclatureId { get; }
		int NomenclatureIdForTerminal { get; }

		int PaidDeliveryNomenclatureId { get; }
		int AdvancedPaymentNomenclatureId { get; }
		int Folder1cForOnlineStoreNomenclatures { get; }
		int MeasurementUnitForOnlineStoreNomenclatures { get; }
		int RootProductGroupForOnlineStoreNomenclatures { get; }
		int CurrentOnlineStoreId { get; }
		decimal GetWaterPriceIncrement { get; }
		string OnlineStoreExportFileUrl { get; }
		int PromotionalNomenclatureGroupId { get; }
		int DailyCoolerRentNomenclatureId { get; }
		int[] PaidDeliveriesNomenclaturesIds { get; }
		int IdentifierOfOnlineShopGroup { get; }


		int WaterSemiozerieId { get; }
		int WaterKislorodnayaId { get; }
		int WaterSnyatogorskayaId { get; }
		int WaterKislorodnayaDeluxeId { get; }
		int WaterStroikaId { get; }
		int WaterRuchkiId { get; }
		int DefaultBottleNomenclatureId { get; }
		int NomenclatureToAddWithMasterId { get; }
		int ForfeitId { get; }
		int FastDeliveryNomenclatureId { get; }
		int MasterCallNomenclatureId { get; }
		int DocumentsNomenclatureId { get; }
	}
}
