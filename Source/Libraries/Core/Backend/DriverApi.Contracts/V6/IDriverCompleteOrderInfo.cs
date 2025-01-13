namespace DriverApi.Contracts.V6
{
	public interface IDriverOrderShipmentInfo : ITrueMarkOrderScannedInfo
	{
		int OrderId { get; }
		int BottlesReturnCount { get; }
		string DriverComment { get; }
	}
}
