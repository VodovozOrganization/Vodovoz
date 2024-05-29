using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Logistic.Reports;

namespace Vodovoz.Logistic.Reports
{
	[ToolboxItem(true)]
	public partial class CarIsNotAtLineReportParametersView
		: DialogViewBase<CarIsNotAtLineReportParametersViewModel>
	{
		public CarIsNotAtLineReportParametersView(CarIsNotAtLineReportParametersViewModel viewModel)
			: base(viewModel)
		{
			Build();
		}
	}
}
