namespace DriverApi.Contracts.V4
{
	public interface IDriverComplaintInfo
	{
		int Rating { get; }
		int DriverComplaintReasonId { get; }
		string OtherDriverComplaintReasonComment { get; }
	}
}
