using Vodovoz.Models.TrueMark;

namespace DriverAPI.Library.Models
{
	public interface IDriverOrderShipmentInfo : ITrueMarkOrderScannedInfo
	{
		int OrderId { get; }
		int BottlesReturnCount { get; }
		string DriverComment { get; }
	}
}
