using System;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.Settings.Database.Logistics
{
	public class NewDriverAdvanceSettings : INewDriverAdvanceSettings
	{
		private readonly ISettingsController _settingsController;

		public NewDriverAdvanceSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int NewDriverAdvanceFirstDay => _settingsController.GetIntValue("new_driver_advance_first_day");
		public int NewDriverAdvanceLastDay => _settingsController.GetIntValue("new_driver_advance_last_day");
		public decimal NewDriverAdvanceSum => _settingsController.GetDecimalValue("new_driver_advance_sum");
		public bool IsNewDriverAdvanceEnabled => _settingsController.GetBoolValue("is_new_driver_advance_enabled");
	}
}
