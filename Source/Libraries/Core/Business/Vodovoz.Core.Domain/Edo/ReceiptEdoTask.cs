using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class ReceiptEdoTask : OrderEdoTask
	{
		private int _cashReceiptId;

		// Допускается связь с чеком, для интеграции со старой системой чеков
		[Display(Name = "Код чека")]
		public virtual int CashReceiptId
		{
			get => _cashReceiptId;
			set => SetField(ref _cashReceiptId, value);
		}

		public override EdoTaskType TaskType => EdoTaskType.Receipt;
	}
}
