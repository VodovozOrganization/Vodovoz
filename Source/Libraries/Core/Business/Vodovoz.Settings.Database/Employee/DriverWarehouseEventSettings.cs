using System;
using Vodovoz.Settings.Employee;

namespace Vodovoz.Settings.Database.Employee
{
	public class DriverWarehouseEventSettings : IDriverWarehouseEventSettings
	{
		private readonly ISettingsController _settingsController;

		public DriverWarehouseEventSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}
		
		public int MaxDistanceMetersFromScanningLocation =>
			_settingsController.GetValue<int>("Events.MaxDistanceMetersFromScanningLocation");
	}
}
