using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Edo
{
	public enum EdoInOrderDocumentGroupType
	{
		[Display(Name = "Эл. первичка")]
		Primary,

		[Display(Name = "Счет")]
		Bill,

		[Display(Name = "Вывод из оборота")]
		Withdrawal
	}
}
