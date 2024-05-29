using QS.Navigation;
using QS.ViewModels.Dialog;
using System;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public class CarIsNotAtLineReportParametersViewModel : DialogViewModelBase
	{
		public CarIsNotAtLineReportParametersViewModel(INavigationManager navigation)
			: base(navigation)
		{
			Title = "Отчёт по простою";
		}

		public DateTime Date { get; set; }

		public int CountDays { get; set; }
	}
}
