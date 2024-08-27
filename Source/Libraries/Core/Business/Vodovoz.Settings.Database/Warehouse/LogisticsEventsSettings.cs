using System;
using Vodovoz.Settings.Warehouse;

namespace Vodovoz.Settings.Database.Warehouse
{
	public class LogisticsEventsSettings : ILogisticsEventsSettings
	{
		private readonly string _parametersPrefix = "LogisticsEvents.";

		private readonly ISettingsController _settingsController;

		public LogisticsEventsSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string BaseUrl =>
			_settingsController.GetStringValue($"{_parametersPrefix}{nameof(BaseUrl)}");

		public int CarLoadDocumentStartLoadEventId =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(CarLoadDocumentStartLoadEventId)}");

		public int CarLoadDocumentEndLoadEventId =>
			_settingsController.GetValue<int>($"{_parametersPrefix}{nameof(CarLoadDocumentEndLoadEventId)}");
	}
}
