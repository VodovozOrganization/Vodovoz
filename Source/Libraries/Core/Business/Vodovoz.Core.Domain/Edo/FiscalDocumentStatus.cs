using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum FiscalDocumentStatus
	{
		[Display(Name = "")]
		None,

		[Display(Name = "В очереди")]
		Queued,

		[Display(Name = "В ожидании")]
		Pending,

		[Display(Name = "Напечатан")]
		Printed,

		[Display(Name = "Уведомляет о завершении")]
		WaitForCallback,

		[Display(Name = "Завершен")]
		Completed,

		[Display(Name = "Проблема")]
		Failed
	}
}
