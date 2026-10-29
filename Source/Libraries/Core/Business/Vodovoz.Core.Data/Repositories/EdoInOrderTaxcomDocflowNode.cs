using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	public class EdoInOrderTaxcomDocflowNode
	{
		public int DocflowId { get; set; }
		public int TaskId { get; set; }
		public DateTime DocflowCreationTime { get; set; }
		public EdoDocumentStatus DocflowStatus { get; set; }
		public DateTime? TaxcomDocflowSendTime { get; set; }
		public Guid? TaxcomDocflowId { get; set; }
		public EdoDocFlowStatus? TaxcomStatus { get; set; }
		public DateTime? LastTaxcomStatusUpdateTime { get; set; }
		public TrueMarkTraceabilityStatus? TaxcomTraceabilityStatus { get; set; }
		public string TaxcomErrorMessage { get; set; }
	}
}
