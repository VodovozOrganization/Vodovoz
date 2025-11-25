using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	public class EquipmentTransferEdoTask : OrderDocumentEdoTask
	{
		public override EdoTaskType TaskType => EdoTaskType.InformalOrderDocument;

		public override OrderDocumentType DocumentType => OrderDocumentType.EquipmentTransfer;
	}
}
