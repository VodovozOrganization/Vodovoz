using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	public class EdoInOrderTransferNode
	{
		public int OrderTaskId { get; set; }
		public int RequestIterationId { get; set; }
		public int RequestId { get; set; }
		public DateTime RequestTime { get; set; }
		public TransferEdoRequestIterationStatus RequestIterationStatus { get; set; }
		public int OrganizationFromId { get; set; }
		public string OrganizationFrom { get; set; }
		public int OrganizationToId { get; set; }
		public string OrganizationTo { get; set; }
		public int TransferTaskId { get; set; }
		public EdoTaskStatus Status { get; set; }
		public EdoTransferTaskStage TransferStage { get; set; }
		public IList<string> TransferedCodes { get; set; }
	}
}
