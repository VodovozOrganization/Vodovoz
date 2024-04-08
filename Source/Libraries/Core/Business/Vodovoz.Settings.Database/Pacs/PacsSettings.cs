using QS.Project.DB;
using System;
using Vodovoz.Settings.Pacs;

namespace Vodovoz.Settings.Database.Pacs
{
	public class PacsSettings : IPacsSettings
	{
		private readonly ISettingsController _settingsController;
		private readonly IDataBaseInfo _dataBaseInfo;

		public PacsSettings(ISettingsController settingsController, IDataBaseInfo dataBaseInfo)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
			_dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
		}

		public TimeSpan OperatorInactivityTimeout => TimeSpan.FromMinutes(_settingsController.GetIntValue($"Pacs.{mode}.OperatorInactivityTimeout.Minutes"));

		public TimeSpan OperatorKeepAliveInterval => TimeSpan.FromMinutes(_settingsController.GetIntValue($"Pacs.{mode}.OperatorKeepAliveInterval.Minutes"));

		public string AdministratorApiUrl => _settingsController.GetStringValue($"Pacs.{mode}.AdministratorApiUrl");

		public string AdministratorApiKey => _settingsController.GetStringValue($"Pacs.{mode}.AdministratorApiKey");

		public string OperatorApiUrl => _settingsController.GetStringValue($"Pacs.{mode}.OperatorApiUrl");

		public string OperatorApiKey => _settingsController.GetStringValue($"Pacs.{mode}.OperatorApiKey");

		private string mode => TestMode ? "Test" : "Work";

		public bool TestMode
		{
			get
			{
				var testDatabase = _settingsController.GetStringValue("Pacs.Test.Database");
				return testDatabase == _dataBaseInfo.Name;
			}
		}
	}
}
