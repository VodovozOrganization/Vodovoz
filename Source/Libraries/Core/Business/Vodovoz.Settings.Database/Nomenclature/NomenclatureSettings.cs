using System;
using System.Linq;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Settings.Database.Nomenclature
{
	public class NomenclatureSettings : INomenclatureSettings
	{
		private readonly ISettingsController _settingsController;

		public NomenclatureSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new System.ArgumentNullException(nameof(settingsController));
		}

		public int[] EquipmentKindsHavingGlassHolder
		{
			get
			{
				var idsString = _settingsController.GetValue<string>(nameof(EquipmentKindsHavingGlassHolder).FromPascalCaseToSnakeCase());

				var ids = idsString.FromStringToIntArray();

				return ids;
			}
		}

		public int[] EquipmentForCheckProductGroupsIds => _settingsController
			.GetValue<string>(nameof(EquipmentForCheckProductGroupsIds))
			.FromStringToIntArray();


		public int ReturnedBottleNomenclatureId => _settingsController.GetIntValue("returned_bottle_nomenclature_id");
		public int NomenclatureIdForTerminal => _settingsController.GetIntValue("terminal_nomenclature_id");

		public int Folder1cForOnlineStoreNomenclatures => _settingsController.GetIntValue("folder_1c_for_online_store_nomenclatures");
		public int PaidDeliveryNomenclatureId => _settingsController.GetIntValue("paid_delivery_nomenclature_id");
		public int AdvancedPaymentNomenclatureId => _settingsController.GetIntValue(nameof(AdvancedPaymentNomenclatureId));
		public int MeasurementUnitForOnlineStoreNomenclatures => _settingsController.GetIntValue("measurement_unit_for_online_store_nomenclatures");
		public int RootProductGroupForOnlineStoreNomenclatures => _settingsController.GetIntValue("root_product_group_for_online_store_nomenclatures");
		public int CurrentOnlineStoreId => _settingsController.GetIntValue("current_online_store_id");
		public decimal GetWaterPriceIncrement => _settingsController.GetDecimalValue("water_price_increment");
		public string OnlineStoreExportFileUrl => _settingsController.GetStringValue("online_store_export_file_url");
		public int FastDeliveryNomenclatureId => _settingsController.GetIntValue("fast_delivery_nomenclature_id");
		public int PromotionalNomenclatureGroupId => _settingsController.GetIntValue("promotional_nomenclature_group_id");
		public int DailyCoolerRentNomenclatureId => _settingsController.GetIntValue(nameof(DailyCoolerRentNomenclatureId));

		public int WaterSemiozerieId => _settingsController.GetIntValue("nomenclature_semiozerie_id");
		public int WaterKislorodnayaId => _settingsController.GetIntValue("nomenclature_kislorodnaya_id");
		public int WaterSnyatogorskayaId => _settingsController.GetIntValue("nomenclature_snyatogorskaya_id");
		public int WaterKislorodnayaDeluxeId => _settingsController.GetIntValue("nomenclature_kislorodnaya_deluxe_id");
		public int WaterStroikaId => _settingsController.GetIntValue("nomenclature_stroika_id");
		public int WaterRuchkiId => _settingsController.GetIntValue("nomenclature_ruchki_id");
		public int DefaultBottleNomenclatureId => _settingsController.GetIntValue("default_bottle_nomenclature");
		public int NomenclatureToAddWithMasterId => _settingsController.GetIntValue("номенклатура_для_выезда_с_мастером");
		public int ForfeitId => _settingsController.GetIntValue("forfeit_nomenclature_id");
		public int MasterCallNomenclatureId => _settingsController.GetIntValue(nameof(MasterCallNomenclatureId));
		public int DocumentsNomenclatureId => _settingsController.GetIntValue(nameof(DocumentsNomenclatureId));
		
		public int IdentifierOfOnlineShopGroup
		{
			get
			{
				string parameterName = "код_группы_товаров_для_интерент-магазина";

				if(!_settingsController.ContainsSetting(parameterName)
				   || !int.TryParse(_settingsController.GetStringValue(parameterName), out int res))
				{
					return 0;
				}

				return res;
			}
		}

		public int[] PaidDeliveriesNomenclaturesIds => new[]
{
			PaidDeliveryNomenclatureId,
			FastDeliveryNomenclatureId
		};
	}
}
