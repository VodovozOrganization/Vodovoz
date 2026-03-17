namespace Mango.Business.Models
{
	public class MangoOperatorReference
	{
		public long OperatorId { get; init; }
		public string? OperatorName { get; init; }
		public string? Extension { get; init; }

		public long GroupId { get; init; }
		public string? GroupName { get; init; }
	}
}
