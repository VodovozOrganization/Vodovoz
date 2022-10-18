using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Enums
{
	public enum ActionsWithInvoice
	{
		[Display(Name = "В раскладке")]
		createdNew,
		[Display(Name = "Удалена")]
		notCreated
	}
}
