using Gamma.ColumnConfig;
using Gtk;
using QS.Views;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ReportsParameters.Cash;
using EmployeeNode  = Vodovoz.ViewModels.ReportsParameters.Cash.DayOfSalaryGiveoutReportViewModel.EmployeeNode;

namespace Vodovoz.Views.ReportsParameters.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DayOfSalaryGiveoutReportView : ViewBase<DayOfSalaryGiveoutReportViewModel>
	{
		public DayOfSalaryGiveoutReportView(DayOfSalaryGiveoutReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			datepicker1.Binding.AddBinding(ViewModel, vm => vm.StartDateTime, w => w.DateOrNull).InitializeFromSource();

			ytreeviewEmployees.ColumnsConfig = FluentColumnsConfig<EmployeeNode>.Create()
				.AddColumn("Код").AddNumericRenderer(d => d.Id)
				.AddColumn("Имя").AddTextRenderer(d => d.FullName)
				.AddColumn("Выбрать").AddToggleRenderer(d => d.IsSelected)
				.RowCells().AddSetter<CellRenderer>(
					(c, n) => c.CellBackgroundGdk = n.Category == EmployeeCategory.forwarder ? GdkColors.InsensitiveBG : GdkColors.PrimaryBase)
				.Finish();
			ytreeviewEmployees.SetItemsSource(ViewModel.EmployeeNodes);

			buttonGenerate.Binding.AddBinding(ViewModel, vm => vm.CanGenerate, w => w.Sensitive).InitializeFromSource();

			buttonGenerate.Clicked += (sender, args) => ViewModel.GenerateReportCommand.Execute();
			buttonInfo.Clicked += (sender, args) => ViewModel.ShowInfo();
			buttonSelectAll.Clicked += (sender, args) =>
			{
				ViewModel.SelectAllCommand.Execute();
				ytreeviewEmployees.SetItemsSource(ViewModel.EmployeeNodes);
			};
			buttonUnselectAll.Clicked += (sender, args) =>
			{
				ViewModel.UnselectAllCommand.Execute();
				ytreeviewEmployees.SetItemsSource(ViewModel.EmployeeNodes);
			};
		}
	}
}
