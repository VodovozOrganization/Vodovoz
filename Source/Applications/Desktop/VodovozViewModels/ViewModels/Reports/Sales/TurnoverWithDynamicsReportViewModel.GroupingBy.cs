using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		/// <summary>
		/// Варианты группировки и отображения отчета
		/// </summary>
		public enum GroupingByEnum
		{
			[Display(Name = "Группировать по номенклатуре")]
			Nomenclature,
			[Display(Name = "Группировать по контрагенту, не показывать контакты")]
			Counterparty,
			[Display(Name = "Группировать по контрагенту, показать контакты")]
			CounterpartyShowContacts
		}
	}
}
