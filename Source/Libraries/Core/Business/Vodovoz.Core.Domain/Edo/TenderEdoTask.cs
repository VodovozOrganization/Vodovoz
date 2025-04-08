namespace Vodovoz.Core.Domain.Edo
{
	public class TenderEdoTask : OrderEdoTask
	{
		public override EdoTaskType TaskType => EdoTaskType.Tender;
	}
}
