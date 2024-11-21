using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeBookkeepingReportsFilterFactory
	{
		[Appellative(
			Nominative = "Тип доставки",
			NominativePlural = "Типы доставки")]
		public enum EdoControlReportOrderDeliveryType
		{
			[Display(Name = "Доставка за час")]
			FastDelivery,
			[Display(Name = "Закр. док")]
			CloseDocument,
			[Display(Name = "Обычная доставка")]
			CommonDelivery,
			[Display(Name = "Самовывоз")]
			SelfDelivery,
		}
	}
}
