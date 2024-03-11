namespace DriverApi.Contracts.V5
{
	public interface IDriverOrderShipmentInfo : ITrueMarkOrderScannedInfo
	{
		int OrderId { get; }
		int BottlesReturnCount { get; }
		string DriverComment { get; }
	}
}
