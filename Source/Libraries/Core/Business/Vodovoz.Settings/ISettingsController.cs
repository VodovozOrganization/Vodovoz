using System.Globalization;
using System;
using System.Runtime.CompilerServices;

namespace Vodovoz.Settings
{
	public interface ISettingsController
	{
		bool ContainsSetting(string settingName);
		string GetStringValue(string settingName);
		bool GetBoolValue(string settingName);
		int GetIntValue(string settingName);
		decimal GetDecimalValue(string settingName);
		char GetCharValue(string settingName);
		DateTime GetDateTimeValue(string settingName, CultureInfo cultureInfo = null);
		T GetValue<T>(string settingName);

		void CreateOrUpdateSetting(string name, string value, TimeSpan? cacheTimeOut = null);
		void RefreshSettings();
	}
}
