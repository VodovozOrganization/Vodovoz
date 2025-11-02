namespace DriverApi.Contracts.V5
{
	public interface IDriverComplaintInfo
	{
		int Rating { get; }
		int DriverComplaintReasonId { get; }
		string OtherDriverComplaintReasonComment { get; }
	}
}
