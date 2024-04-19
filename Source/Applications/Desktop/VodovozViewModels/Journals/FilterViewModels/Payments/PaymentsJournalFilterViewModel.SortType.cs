using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Filters.ViewModels
{
	public partial class PaymentsJournalFilterViewModel
	{
		public enum PaymentJournalSortType
		{
			[Display(Name = "По статусу распределения")]
			Status,
			[Display(Name = "По дате")]
			Date,
			[Display(Name = "По номеру")]
			PaymentNum,
			[Display(Name = "По сумме")]
			TotalSum
		}
	}
}
