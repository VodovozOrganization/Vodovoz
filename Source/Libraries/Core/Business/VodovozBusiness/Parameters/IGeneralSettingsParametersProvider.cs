namespace Vodovoz.Parameters
{
	public interface IGeneralSettingsParametersProvider
	{
		string GetRouteListPrintedFormPhones { get; }
		void UpdateRouteListPrintedFormPhones(string text);

		bool GetCanAddForwardersToLargus { get; }
		void UpdateCanAddForwardersToLargus(bool value);
	}
}
