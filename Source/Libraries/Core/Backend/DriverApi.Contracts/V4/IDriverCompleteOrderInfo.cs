namespace DriverApi.Contracts.V4
{
	public interface IDriverOrderShipmentInfo : ITrueMarkOrderScannedInfo
	{
		int OrderId { get; }
		int BottlesReturnCount { get; }
		string DriverComment { get; }
	}
}
