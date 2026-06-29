using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	public class TransferEdoTaskNode
	{
		public int OrderTaskId { get; set; }
		public DateTime RequestTime { get; set; }
		public int TransferTaskId { get; set; }
		public int OrganizationFromId { get; set; }
		public string OrganizationFrom { get; set; }
		public int OrganizationToId { get; set; }
		public string OrganizationTo { get; set; }
		public EdoTaskStatus Status { get; set; }
	}
}
