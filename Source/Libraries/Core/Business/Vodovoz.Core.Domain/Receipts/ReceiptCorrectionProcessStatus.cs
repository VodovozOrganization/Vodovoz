using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Receipts
{
	public enum ReceiptCorrectionProcessStatus
	{
		[Display(Name = "Ожидает фискализации")]
		Pending,

		[Display(Name = "В работе")]
		InProgress,

		[Display(Name = "Успешно")]
		Completed,

		[Display(Name = "Ошибка")]
		Failed
	}
}
