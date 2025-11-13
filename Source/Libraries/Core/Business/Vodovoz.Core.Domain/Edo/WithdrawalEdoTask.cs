namespace Vodovoz.Core.Domain.Edo
{
	public class WithdrawalEdoTask : OrderEdoTask
	{
		public override EdoTaskType TaskType => EdoTaskType.Withdrawal;

	}
}
