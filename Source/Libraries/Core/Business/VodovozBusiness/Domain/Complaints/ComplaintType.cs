using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
	public enum ComplaintType
	{
		[Display(Name = "Внутренняя")]
		Inner,
		[Display(Name = "Клиентская")]
		Client,
		[Display(Name = "Водительская")]
		Driver
	}
}
