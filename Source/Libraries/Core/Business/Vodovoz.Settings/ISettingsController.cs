﻿namespace Vodovoz.Settings
{
	public interface ISettingsController
	{
		bool ContainsSetting(string settingName);
		string GetStringValue(string settingName);
		bool GetBoolValue(string settingName);
		int GetIntValue(string settingName);
		decimal GetDecimalValue(string settingName);
		char GetCharValue(string settingName);
		T GetValue<T>(string settingName);

		void CreateOrUpdateSetting(string name, string value);
		void RefreshSettings();
	}
}
