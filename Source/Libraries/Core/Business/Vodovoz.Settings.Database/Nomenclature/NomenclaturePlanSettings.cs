using System;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Settings.Database.Nomenclature
{
	public class NomenclaturePlanSettings : INomenclaturePlanSettings
	{
		private readonly ISettingsController _settingsController;

		public NomenclaturePlanSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int CallCenterSubdivisionId => _settingsController.GetIntValue("call_center_subdivision_id");
	}
}
