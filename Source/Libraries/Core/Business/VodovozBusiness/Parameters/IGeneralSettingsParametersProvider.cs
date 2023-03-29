namespace Vodovoz.Parameters
{
	public interface IGeneralSettingsParametersProvider
	{
		string GetRouteListPrintedFormPhones { get; }
		void UpdateRouteListPrintedFormPhones(string text);

		bool GetCanAddForwardersToLargus { get; }
		string OrderAutoComment { get; }
		void UpdateCanAddForwardersToLargus(bool value);
		void UpdateOrderAutoComment(string value);

		int[] SubdivisionsToInformComplaintHasNoDriver { get; }
		void UpdateSubdivisionsToInformComplaintHasNoDriver(int[] subdivisionIds);
	}
}
