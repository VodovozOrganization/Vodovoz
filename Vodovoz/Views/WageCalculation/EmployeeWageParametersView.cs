using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;
using Gamma.ColumnConfig;
namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeWageParametersView : WidgetViewBase<EmployeeWageParametersViewModel>
	{
		public EmployeeWageParametersView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			treeViewWageParameters.ColumnsConfig = FluentColumnsConfig<EmployeeWageParameterNode>.Create()
				.AddColumn("Тип расчета").AddTextRenderer(x => x.WageType)
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDate)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDate)
				.Finish();

			ViewModel.OnParameterNodesUpdated += (sender, e) => UpdateWageParameters();
			treeViewWageParameters.RowActivated += (o, args) => ViewModel.OpenWageParameterCommand.Execute(treeViewWageParameters.GetSelectedObject() as EmployeeWageParameterNode);
			treeViewWageParameters.Selection.Changed += (sender, e) => {
				ViewModel.ChangeWageParameterCommand.RaiseCanExecuteChanged();
				ViewModel.ChangeWageStartDateCommand.RaiseCanExecuteChanged();
				ViewModel.OpenWageParameterCommand.RaiseCanExecuteChanged();
			};


			ydatepickerStart.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date).InitializeFromSource();

			buttonChangeWageParameter.Clicked += (sender, e) => ViewModel.ChangeWageParameterCommand.Execute();
			ViewModel.ChangeWageParameterCommand.CanExecuteChanged += (sender, e) => buttonChangeWageParameter.Sensitive = ViewModel.ChangeWageParameterCommand.CanExecute();
			buttonChangeWageParameter.Sensitive = ViewModel.ChangeWageParameterCommand.CanExecute();

			buttonChangeDate.Clicked += (sender, e) => ViewModel.ChangeWageStartDateCommand.Execute(GetSelectedNode());
			ViewModel.ChangeWageStartDateCommand.CanExecuteChanged += (sender, e) => buttonChangeDate.Sensitive = ViewModel.ChangeWageStartDateCommand.CanExecute(GetSelectedNode());
			buttonChangeDate.Sensitive = ViewModel.ChangeWageStartDateCommand.CanExecute(GetSelectedNode());

			UpdateWageParameters();
		}

		private EmployeeWageParameterNode GetSelectedNode()
		{
			return treeViewWageParameters.GetSelectedObject() as EmployeeWageParameterNode;
		}

		private void UpdateWageParameters()
		{
			treeViewWageParameters.ItemsDataSource = ViewModel.WageParameterNodes;
		}

	}
}
