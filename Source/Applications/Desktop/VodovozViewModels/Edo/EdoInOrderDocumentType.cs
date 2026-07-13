using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Edo
{
	public enum EdoInOrderDocumentType
	{
		[Display(Name = "УПД")]
		Upd,

		[Display(Name = "Чек")]
		Receipt,

		[Display(Name = "Тендер")]
		Tender,

		[Display(Name = "Вывод из оборота")]
		Withdrawal,

		[Display(Name = "Забор кодов")]
		SaveCode,

		[Display(Name = "Счет")]
		Bill,
	}
}
