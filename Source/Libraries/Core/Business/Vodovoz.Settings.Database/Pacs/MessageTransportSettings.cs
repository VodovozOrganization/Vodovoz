using QS.Project.DB;
using System;
using System.Collections.Generic;
using Vodovoz.Settings.Pacs;

namespace Vodovoz.Settings.Database.Pacs
{
	public class MessageTransportSettings : IMessageTransportSettings
	{
		private readonly ISettingsController _settingsController;
		private readonly IDataBaseInfo _dataBaseInfo;

		public MessageTransportSettings(ISettingsController settingsController, IDataBaseInfo dataBaseInfo)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
			_dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
		}

		public List<MessageTTLSetting> MessagesTimeToLive => new List<MessageTTLSetting>();

		public string Host => _settingsController.GetStringValue($"Pacs.{mode}.MessageTranport.Host");
		public int Port => _settingsController.GetIntValue($"Pacs.{mode}.MessageTranport.Port");
		public string VirtualHost => _settingsController.GetStringValue($"Pacs.{mode}.MessageTranport.VirtualHost");
		public string Username => _settingsController.GetStringValue($"Pacs.{mode}.MessageTranport.Username");
		public string Password => _settingsController.GetStringValue($"Pacs.{mode}.MessageTranport.Password");
		public bool UseSSL => _settingsController.GetBoolValue($"Pacs.{mode}.MessageTranport.UseSSL");

		private string mode => TestMode? "Test" : "Work";

		public bool TestMode
		{
			get
			{
				var testDatabase = _settingsController.GetStringValue("Pacs.Test.Database");
				return testDatabase == _dataBaseInfo.Name;
			}
		}

		/// <summary>
		/// Нельзя устанавливать в БД
		/// </summary>
		public string AllowSslPolicyErrors => "";
	}
}
