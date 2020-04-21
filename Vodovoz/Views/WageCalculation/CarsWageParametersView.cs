using System;
using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarsWageParametersView : TabViewBase<CarsWageParametersViewModel>
	{
		public CarsWageParametersView(CarsWageParametersViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		protected void Configure()
		{
			treeViewWageParameters.ColumnsConfig = FluentColumnsConfig<WageParameterNode>.Create()
				.AddColumn("Тип расчета").AddTextRenderer(x => x.WageType)
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDate)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDate)
				.Finish();

			ViewModel.OnParameterNodesUpdated += (sender, e) => UpdateWageParameters();

			treeViewWageParameters.RowActivated += (o, args) => ViewModel.OpenWageParameterCommand.Execute(treeViewWageParameters.GetSelectedObject() as WageParameterNode);
			treeViewWageParameters.Selection.Changed += (sender, e) => {
				ViewModel.ChangeWageParameterCommand.RaiseCanExecuteChanged();
				ViewModel.ChangeWageStartDateCommand.RaiseCanExecuteChanged();
				ViewModel.OpenWageParameterCommand.RaiseCanExecuteChanged();
			};

			buttonChangeWageParameter.Clicked += (sender, e) => ViewModel.ChangeWageParameterCommand.Execute();
			ViewModel.ChangeWageParameterCommand.CanExecuteChanged += (sender, e) => buttonChangeWageParameter.Sensitive = ViewModel.ChangeWageParameterCommand.CanExecute();
			buttonChangeWageParameter.Sensitive = ViewModel.ChangeWageParameterCommand.CanExecute();

			buttonChangeDate.Clicked += (sender, e) => ViewModel.ChangeWageStartDateCommand.Execute(GetSelectedNode());
			ViewModel.ChangeWageStartDateCommand.CanExecuteChanged += (sender, e) => buttonChangeDate.Sensitive = ViewModel.ChangeWageStartDateCommand.CanExecute(GetSelectedNode());
			buttonChangeDate.Sensitive = ViewModel.ChangeWageStartDateCommand.CanExecute(GetSelectedNode());

			ydatepickerStart.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.Date).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false, QS.Navigation.CloseSource.Cancel); };

			UpdateWageParameters();
		}

		private WageParameterNode GetSelectedNode()
		{
			return treeViewWageParameters.GetSelectedObject() as WageParameterNode;
		}

		private void UpdateWageParameters()
		{
			treeViewWageParameters.ItemsDataSource = ViewModel.WageParameterNodes;
		}
	}
}
