namespace TaxcomEdoApi.Library.Models.Containers
{
	public enum DocflowStatus
	{
		Unknown,
		InProgress,
		Succeed,
		Warning,
		Error,
		NotStarted,
		CompletedWithDivergences,
		NotAccepted,
		WaitingForCancellation,
		Cancelled
	}
}
