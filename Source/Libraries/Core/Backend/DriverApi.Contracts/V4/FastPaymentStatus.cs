using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V4
{
	public enum FastPaymentStatus
	{
		[Display(Name = "Обрабатывается")]
		Processing = 1,
		[Display(Name = "Отбракован")]
		Rejected,
		[Display(Name = "Исполнен")]
		Performed
	}
}
