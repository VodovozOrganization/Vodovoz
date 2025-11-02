using System;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	public class ErrorSendSettings : IErrorSendSettings
	{
		private readonly ISettingsController _settingsController;

		public ErrorSendSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string DefaultBaseForErrorSend => _settingsController.GetStringValue("base_for_error_send");

		public int RowCountForErrorLog => _settingsController.GetIntValue("row_count_for_error_log");
	}
}
