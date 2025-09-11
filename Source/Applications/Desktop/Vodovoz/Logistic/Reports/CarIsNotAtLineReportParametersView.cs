using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Logistic.Reports;

namespace Vodovoz.Logistic.Reports
{
	[ToolboxItem(true)]
	public partial class CarIsNotAtLineReportParametersView
		: DialogViewBase<CarIsNotAtLineReportParametersViewModel>
	{
		public CarIsNotAtLineReportParametersView(
			CarIsNotAtLineReportParametersViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			datepickerDate.Binding
				.AddBinding(ViewModel, vm => vm.Date, w => w.Date)
				.InitializeFromSource();

			datepickerDate.IsEditable = true;

			yspinbuttonDaysCount.Binding
				.AddBinding(ViewModel, vm => vm.CountDays, w => w.ValueAsInt)
				.InitializeFromSource();

			vboxFilter.Remove(includeexcludefiltergroupview1);
			includeexcludefiltergroupview1 = new Presentation.Views.IncludeExcludeFilterGroupView(ViewModel.IncludeExludeFilterGroupViewModel);
			includeexcludefiltergroupview1.Show();
			vboxFilter.Add(includeexcludefiltergroupview1);

			ybuttonGenerate.BindCommand(ViewModel.GenerateAndSaveReportCommand);
		}
	}
}
