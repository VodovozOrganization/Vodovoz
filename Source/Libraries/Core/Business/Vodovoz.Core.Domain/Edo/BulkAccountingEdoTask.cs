namespace Vodovoz.Core.Domain.Edo
{
	public class BulkAccountingEdoTask : OrderEdoTask
	{
		public override EdoTaskType TaskType => EdoTaskType.BulkAccounting;
	}
}
