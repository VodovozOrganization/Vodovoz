using System;
using Vodovoz.Settings.Car;

namespace Vodovoz.Settings.Database.Car
{
	public class CarSettings : ICarSettings
	{
		private readonly ISettingsController _settingsController;

		public CarSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int FirstInSelectionListCarModelId => _settingsController.GetValue<int>(nameof(FirstInSelectionListCarModelId).FromPascalCaseToSnakeCase());
	}
}
