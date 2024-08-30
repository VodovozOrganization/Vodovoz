using System;
using Vodovoz.Settings.Warehouse;

namespace Vodovoz.Settings.Database.Warehouse
{
	public class CarLoadDocumentLoadingProcessSettings : ICarLoadDocumentLoadingProcessSettings
	{
		private readonly string _parametersPrefix = "LoadingProcessSettings.";

		private readonly ISettingsController _settingsController;
		public CarLoadDocumentLoadingProcessSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public TimeSpan NoLoadingActionsTimeout =>
			_settingsController.GetValue<TimeSpan>($"{_parametersPrefix}{nameof(NoLoadingActionsTimeout)}");
	}
}
