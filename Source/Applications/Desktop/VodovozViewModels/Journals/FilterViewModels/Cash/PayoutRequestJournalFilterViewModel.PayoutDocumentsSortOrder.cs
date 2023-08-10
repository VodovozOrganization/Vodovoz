using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Journals.FilterViewModels
{
	public partial class PayoutRequestJournalFilterViewModel
	{
		public enum PayoutDocumentsSortOrder
		{
			[Display(Name = "По дате создания")]
			ByCreationDate,

			[Display(Name = "По дате выдачи")]
			ByMoneyTransferDate
		}
	}
}
