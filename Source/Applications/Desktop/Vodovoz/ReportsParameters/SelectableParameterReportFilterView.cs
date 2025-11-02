using System;
using Vodovoz.ViewModels.Reports;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Gamma.ColumnConfig;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using QS.Views;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SelectableParameterReportFilterView : ViewBase<SelectableParameterReportFilterViewModel>
	{
		public SelectableParameterReportFilterView(SelectableParameterReportFilterViewModel viewModel)
			: base(viewModel)
		{
			Build();
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			ytreeviewParameterSets.Selection.Changed += Selection_Changed;
			search.TextChanged += Search_TextChanged;

			ConfigureBindings();

			ytreeviewParameterSets.ItemsDataSource = ViewModel.ReportFilter.ParameterSets;
			ytreeviewParameterSets.ColumnsConfig = FluentColumnsConfig<SelectableParameterSet>.Create()
				.AddColumn("Параметр").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewParameters.ColumnsConfig = FluentColumnsConfig<SelectableParameter>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.Selected).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Title)
				.Finish();

			ytreeviewParameters.HeadersVisible = false;
			ytreeviewParameterSets.HeadersVisible = false;
		}

		private void ConfigureBindings()
		{
			buttonInclude.Clicked += (sender, e) => ViewModel.SwitchToIncludeCommand.Execute();
			ViewModel.SwitchToIncludeCommand.CanExecuteChanged += (sender, e) => buttonInclude.Sensitive = ViewModel.SwitchToIncludeCommand.CanExecute();

			buttonExclude.Clicked += (sender, e) => ViewModel.SwitchToExcludeCommand.Execute();
			ViewModel.SwitchToExcludeCommand.CanExecuteChanged += (sender, e) => buttonExclude.Sensitive = ViewModel.SwitchToExcludeCommand.CanExecute();

			ytreeviewParameters.Binding.AddBinding(ViewModel, vm => vm.HasSelectedSet, w => w.Sensitive).InitializeFromSource();

			buttonSelectAll.Clicked += (sender, e) => ViewModel.SelectAllParametersCommand.Execute();
			ViewModel.SelectAllParametersCommand.CanExecuteChanged += (sender, e) => buttonSelectAll.Sensitive = ViewModel.SelectAllParametersCommand.CanExecute();

			buttonUnselect.Clicked += (sender, e) => ViewModel.UnselectAllParametersCommand.Execute();
			ViewModel.UnselectAllParametersCommand.CanExecuteChanged += (sender, e) => buttonUnselect.Sensitive = ViewModel.UnselectAllParametersCommand.CanExecute();
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(ViewModel.CurrentParameterSet):
					UpdateCurrentParameterSetFromSource();
					break;
				case nameof(ViewModel.HasSelectedSet):
					UpdateSearchSensitivity();
					break;
				case nameof(ViewModel.SearchValue):
					UpdateSearchText();
					break;
				default:
					break;
			}
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			UpdateCurrentParameterSetToSource();
		}

		private void UpdateCurrentParameterSetToSource()
		{
			SelectableParameterSet selectedParameterSet = ytreeviewParameterSets.GetSelectedObject() as SelectableParameterSet;
			if(selectedParameterSet != null && selectedParameterSet == ViewModel.CurrentParameterSet) {
				return;
			}

			ViewModel.CurrentParameterSet = selectedParameterSet;
			if(ViewModel.CurrentParameterSet != null) {
				ViewModel.CurrentParameterSet.Parameters.ListContentChanged -= CurrentParameterSet_ListContentChanged;
				ViewModel.CurrentParameterSet.Parameters.ListContentChanged += CurrentParameterSet_ListContentChanged;
			}
		}

		void CurrentParameterSet_ListContentChanged(object sender, EventArgs e)
		{
			ytreeviewParameters.QueueDraw();
		}

		private void UpdateCurrentParameterSetFromSource()
		{
			RefreshParametersSource();
			RefreshButtons();

			SelectableParameterSet selectedParameterSet = ytreeviewParameterSets.GetSelectedObject() as SelectableParameterSet;
			if(selectedParameterSet != null && selectedParameterSet == ViewModel.CurrentParameterSet) {
				return;
			}

			if(ViewModel.CurrentParameterSet == null)
			{
				return;
			}
			var selectedIter = ytreeviewParameterSets.YTreeModel.IterFromNode(ViewModel.CurrentParameterSet);
			if(selectedIter.UserData == IntPtr.Zero) {
				return;
			}
			ytreeviewParameterSets.Selection.SelectIter(selectedIter);
		}

		private void RefreshButtons()
		{
			if(ViewModel.CurrentParameterSet?.FilterType == SelectableFilterType.Exclude) {
				buttonExclude.Active = true;
			} else {
				buttonInclude.Active = true;
			}
		}
		
		private void UpdateSearchText()
		{
			search.Text = ViewModel.SearchValue;
			RefreshParametersSource();
		}

		private void RefreshParametersSource()
		{
			if(ViewModel?.CurrentParameterSet == null) {
				ytreeviewParameters.ItemsDataSource = null;
				return;
			}

			var source = GetCurrentParameters();

			if(ViewModel.CurrentParameterSet.Parameters.Any(x => x.Children.Any())) {
				var recursiveModel = new RecursiveTreeModel<SelectableParameter>(source, x => x.Parent, x => x.Children);
				ytreeviewParameters.YTreeModel = recursiveModel;
			} else {
				ytreeviewParameters.ItemsDataSource = source;
			}
		}

		private void UpdateSearchSensitivity()
		{
			search.Sensitive = ViewModel.HasSelectedSet;
		}

		void Search_TextChanged(object sender, EventArgs e)
		{
			ViewModel.SilentUpdateSearchValue(search.Text);
			RefreshParametersSource();
		}

		private IList<SelectableParameter> GetCurrentParameters()
		{
			if(ViewModel?.CurrentParameterSet?.Parameters == null) {
				return new List<SelectableParameter>();
			}

			ViewModel.CurrentParameterSet.FilterParameters(search.Text);
			return ViewModel.CurrentParameterSet.OutputParameters;
		}
	}
}
