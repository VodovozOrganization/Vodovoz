namespace DriverAPI.Library.Models
{
	public interface IDriverComplaintInfo
	{
		int Rating { get; }
		int DriverComplaintReasonId { get; }
		string OtherDriverComplaintReasonComment { get; }
	}
}
