using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeBookkeepingReportsFilterFactory
	{
		[Appellative(
			Nominative = "Тип переноса заказа",
			NominativePlural = "Типы переносов заказов")]
		public enum EdoControlReportAddressTransferType
		{
			[Display(Name = "С допогрузкой на складе")]
			NeedToReload,
			[Display(Name = "С передачей товара от водителя")]
			FromHandToHand,
			[Display(Name = "Из свободных остатков получателя")]
			FromFreeBalance,
			[Display(Name = "Без переноса")]
			NoTransfer
		}
	}
}
