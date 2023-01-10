using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarsMonitoringView : TabViewBase<CarsMonitoringViewModel>
	{
		public CarsMonitoringView(CarsMonitoringViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
