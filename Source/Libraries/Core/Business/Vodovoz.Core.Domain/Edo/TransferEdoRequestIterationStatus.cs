using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public enum TransferEdoRequestIterationStatus
	{
		[Display(Name = "В процессе")]
		InProgress,

		[Display(Name = "Завершен")]
		Completed,
	}
}
