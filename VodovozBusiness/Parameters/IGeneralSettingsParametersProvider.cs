namespace Vodovoz.Parameters
{
	public interface IGeneralSettingsParametersProvider
	{
		string GetRouteListPrintedFormPhones { get; }
		void UpdateRouteListPrintedFormPhones(string text);
	}
}
