namespace Vodovoz.Parameters
{
    public interface IParametersProvider
    {
        bool ContainsParameter(string parameterName);
        void CreateOrUpdateParameter(string name, string value);
        char GetCharValue(string parameterId);
        decimal GetDecimalValue(string parameterId);
        int GetIntValue(string parameterId);
        string GetParameterValue(string parameterName);
        string GetStringValue(string parameterId);
        void RefreshParameters();
    }
}