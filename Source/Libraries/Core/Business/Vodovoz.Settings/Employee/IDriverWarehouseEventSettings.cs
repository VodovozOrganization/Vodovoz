namespace Vodovoz.Settings.Employee
{
	public interface IDriverWarehouseEventSettings
	{
		int MaxDistanceMetersFromScanningLocation { get; }
		string VodovozSiteForQr { get; }
	}
}
