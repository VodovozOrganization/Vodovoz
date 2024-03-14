using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ViewModels.Payments.PaymentsDiscrepanciesAnalysis
{
	public partial class PaymentsDiscrepanciesAnalysisViewModel
	{
		public enum DiscrepancyCheckMode
		{
			[Display(Name = "Сверка по контрагенту")]
			ReconciliationByCounterparty,
			[Display(Name = "Общая сверка")]
			CommonReconciliation
		}
	}
}
