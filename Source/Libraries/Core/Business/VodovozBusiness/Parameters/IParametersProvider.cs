using System;

namespace Vodovoz.Parameters
{
	[Obsolete("Вместо него необходимо использовать класс Vodovoz.Settings.Database.SettingsController")]
	public interface IParametersProvider
    {
        bool ContainsParameter(string parameterName);
        void CreateOrUpdateParameter(string name, string value);
		string GetParameterValue(string parameterName, bool allowEmpty = false);
		char GetCharValue(string parameterId);
        decimal GetDecimalValue(string parameterId);
        int GetIntValue(string parameterId);
        string GetStringValue(string parameterId);
        bool GetBoolValue(string parameterId);
        T GetValue<T>(string parameterId);

        void RefreshParameters();
	}
}
