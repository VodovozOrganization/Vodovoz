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
	}
}
