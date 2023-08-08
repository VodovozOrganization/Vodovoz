using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public enum GroupingByEnum
		{
			[Display(Name = "Номенклатура")]
			Nomenclature,
			[Display(Name = "Контрагент")]
			Counterparty,
			[Display(Name = "Контрагент, показать контакты")]
			CounterpartyShowContacts
		}
	}
}
