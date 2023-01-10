using Vodovoz.Models.TrueMark;

namespace DriverAPI.Library.Models
{
	public interface IDriverCompleteOrderInfo : ITrueMarkOrderScannedInfo
	{
		int OrderId { get; }
		int BottlesReturnCount { get; }
		int Rating { get; }
		int DriverComplaintReasonId { get; }
		string OtherDriverComplaintReasonComment { get; }
		string DriverComment { get; }
	}
}
