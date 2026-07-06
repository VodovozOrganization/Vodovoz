using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	public class EdoDocflowForOrderNode
	{
		public int OrderTaskId { get; set; }
		public int TransferTaskId { get; set; }
		public EdoType EdoType { get; set; }
		public int TaxcomDocumentId { get; set; }
		public string TaxcomDocflowId { get; set; }
		public EdoDocFlowStatus TaxcomDocflowStatus { get; set; }
	}
}
