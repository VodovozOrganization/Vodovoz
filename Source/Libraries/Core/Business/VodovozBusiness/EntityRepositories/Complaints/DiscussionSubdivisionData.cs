using Vodovoz.Domain.Complaints;

namespace Vodovoz.EntityRepositories.Complaints
{
	public class DiscussionSubdivisionData
	{
		public int DiscussionId { get; set; }
		public int ComplaintId { get; set; }
		public int SubdivisionId { get; set; }
		public ComplaintDiscussionStatuses DiscussionStatuse { get; set; }
	}
}
