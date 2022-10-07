using System;
using QS.ViewModels;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using System.Linq;
using QS.Commands;

namespace Vodovoz.ViewModels.Reports
{
	public class SelectableParameterReportFilterViewModel : WidgetViewModelBase
	{
		public SelectableParametersReportFilter ReportFilter;

		private SelectableParameterSet currentParameterSet;
		public virtual SelectableParameterSet CurrentParameterSet {
			get => currentParameterSet;
			set {
				if(!ReportFilter.ParameterSets.Contains(value)) {
					value = null;
				}
				var oldParameterSet = currentParameterSet;
				if(SetField(ref currentParameterSet, value)) {
					OnPropertyChanged(nameof(HasSelectedSet));
					if(oldParameterSet != null) {
						oldParameterSet.PropertyChanged -= CurrentParameterSet_PropertyChanged;
						oldParameterSet.SelectionChanged -= OnSelectionChanged;
					}
					if(currentParameterSet != null) {
						currentParameterSet.PropertyChanged += CurrentParameterSet_PropertyChanged;
						currentParameterSet.SelectionChanged += OnSelectionChanged;
					}
				}
			}
		}

		public bool HasSelectedSet => CurrentParameterSet != null;

		public bool CanSelectAllParameters => CurrentParameterSet != null && CurrentParameterSet.Parameters.Any();

		public bool CanDeselectAllParameters => CurrentParameterSet != null && CurrentParameterSet.Parameters.Any();


		public SelectableParameterReportFilterViewModel(SelectableParametersReportFilter reportFilter)
		{
			this.ReportFilter = reportFilter;
		}

		void CurrentParameterSet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(CurrentParameterSet.Parameters):
					UpdateCurrentSetsParameters();
					break;
				default:
					break;
			}
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			SelectAllParametersCommand.RaiseCanExecuteChanged();
			UnselectAllParametersCommand.RaiseCanExecuteChanged();
		}

		private void UpdateCurrentSetsParameters()
		{
			OnPropertyChanged(nameof(CanSelectAllParameters));
			OnPropertyChanged(nameof(CanDeselectAllParameters));
		}

		private DelegateCommand switchToIncludeCommand;
		public DelegateCommand SwitchToIncludeCommand {
			get {
				if(switchToIncludeCommand == null) {
					switchToIncludeCommand = new DelegateCommand(
						() => {
							CurrentParameterSet.FilterType = SelectableFilterType.Include;
						},
						() => CurrentParameterSet != null
					);
					switchToIncludeCommand.CanExecuteChangedWith(this, x => x.CurrentParameterSet, x => x.HasSelectedSet);
				}
				return switchToIncludeCommand;
			}
		}

		private DelegateCommand switchToExcludeCommand;
		public DelegateCommand SwitchToExcludeCommand {
			get {
				if(switchToExcludeCommand == null) {
					switchToExcludeCommand = new DelegateCommand(
						() => {
							CurrentParameterSet.FilterType = SelectableFilterType.Exclude;
						},
						() => CurrentParameterSet != null
					);
					switchToExcludeCommand.CanExecuteChangedWith(this, x => x.CurrentParameterSet, x => x.HasSelectedSet);
				}
				return switchToExcludeCommand;
			}
		}

		private DelegateCommand selectAllParametersCommand;
		public DelegateCommand SelectAllParametersCommand {
			get {
				if(selectAllParametersCommand == null) {
					selectAllParametersCommand = new DelegateCommand(
						() => {
							CurrentParameterSet.SelectAll();
						},
						() => CurrentParameterSet != null && CurrentParameterSet.Parameters.Any(x => !x.Selected)
					);
					selectAllParametersCommand.CanExecuteChangedWith(this, x => x.CurrentParameterSet);
				}
				return selectAllParametersCommand;
			}
		}

		private DelegateCommand unselectAllParametersCommand;
		public DelegateCommand UnselectAllParametersCommand {
			get {
				if(unselectAllParametersCommand == null) {
					unselectAllParametersCommand = new DelegateCommand(
						() => {
							CurrentParameterSet.UnselectAll();
						},
						() => CurrentParameterSet != null && CurrentParameterSet.Parameters.Any(x => x.Selected)
					);
					unselectAllParametersCommand.CanExecuteChangedWith(this, x => x.CurrentParameterSet);
				}
				return unselectAllParametersCommand;
			}
		}
	}
}
