using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	public class SlaveDbConnectionSettings : ISlaveDbConnectionSettings
	{
		private readonly ISettingsController _settingsController;

		public SlaveDbConnectionSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public bool SlaveConnectionEnabled => _settingsController.GetBoolValue("slave_connection_enabled");

		public string SlaveConnectionEnabledForThisDatabase => _settingsController.GetStringValue("slave_connection_enabled_for_this_database");

		public string SlaveConnectionHost => _settingsController.GetStringValue("slave_connection_host");

		public int SlaveConnectionPort => _settingsController.GetIntValue("slave_connection_port");
	}
}
