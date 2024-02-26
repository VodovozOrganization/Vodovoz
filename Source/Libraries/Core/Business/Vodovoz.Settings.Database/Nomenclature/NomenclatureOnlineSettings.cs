using System;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Settings.Database.Nomenclature
{
	public class NomenclatureOnlineSettings : INomenclatureOnlineSettings
	{
		private readonly ISettingsController _settingsController;

		public NomenclatureOnlineSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		#region Онлайн группы товаров

		public int WaterNomenclatureOnlineGroupId => _settingsController.GetIntValue("WaterNomenclatureOnlineGroupId");
		public int EquipmentNomenclatureOnlineGroupId => _settingsController.GetIntValue("EquipmentNomenclatureOnlineGroupId");
		public int OtherGoodsNomenclatureOnlineGroupId => _settingsController.GetIntValue("OtherGoodsNomenclatureOnlineGroupId");

		#endregion

		#region Онлайн типы товаров

		public int KulerNomenclatureOnlineCategoryId => _settingsController.GetIntValue("KulerNomenclatureOnlineCategoryId");
		public int PurifierNomenclatureOnlineCategoryId => _settingsController.GetIntValue("PurifierNomenclatureOnlineCategoryId");
		public int WaterPumpNomenclatureOnlineCategoryId => _settingsController.GetIntValue("WaterPumpNomenclatureOnlineCategoryId");
		public int CupHolderNomenclatureOnlineCategoryId => _settingsController.GetIntValue("CupHolderNomenclatureOnlineCategoryId");

		#endregion
	}
}
