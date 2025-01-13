namespace DriverApi.Contracts.V6
{
	public interface IDriverComplaintInfo
	{
		int Rating { get; }
		int DriverComplaintReasonId { get; }
		string OtherDriverComplaintReasonComment { get; }
	}
}
