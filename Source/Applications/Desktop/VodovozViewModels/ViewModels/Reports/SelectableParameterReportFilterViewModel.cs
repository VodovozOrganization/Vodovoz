using QS.Commands;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;

namespace Vodovoz.ViewModels.Reports
{
	public class SelectableParameterReportFilterViewModel : WidgetViewModelBase
	{
		private SelectableParameterSet _currentParameterSet;
		private string _searchValue;

		private DelegateCommand _switchToIncludeCommand;
		private DelegateCommand _switchToExcludeCommand;
		private DelegateCommand _selectAllParametersCommand;
		private DelegateCommand _unselectAllParametersCommand;

		public SelectableParameterReportFilterViewModel(SelectableParametersReportFilter reportFilter)
		{
			ReportFilter = reportFilter;
		}

		public event EventHandler<SelectableParameterReportFilterSelectionChangedArgs> SelectionChanged;
		public event EventHandler<FilterTypeChangedArgs> FilterModeChanged;

		public SelectableParametersReportFilter ReportFilter { get; set; }

		public virtual SelectableParameterSet CurrentParameterSet
		{
			get => _currentParameterSet;
			set
			{
				if(!ReportFilter.ParameterSets.Contains(value))
				{
					value = null;
				}

				var oldParameterSet = _currentParameterSet;

				if(SetField(ref _currentParameterSet, value))
				{
					OnPropertyChanged(nameof(HasSelectedSet));

					if(oldParameterSet != null)
					{
						oldParameterSet.PropertyChanged -= OnCurrentParameterSet_PropertyChanged;
						oldParameterSet.SelectionChanged -= OnCurrentParameterSet_SelectionChanged;
					}

					if(_currentParameterSet != null)
					{
						_currentParameterSet.PropertyChanged += OnCurrentParameterSet_PropertyChanged;
						_currentParameterSet.SelectionChanged += OnCurrentParameterSet_SelectionChanged;
					}
				}
			}
		}

		public string SearchValue
		{
			get => _searchValue;
			set => SetField(ref _searchValue, value);
		}

		public bool HasSelectedSet => CurrentParameterSet != null;

		public bool CanSelectAllParameters => CurrentParameterSet != null && CurrentParameterSet.Parameters.Any();

		public bool CanDeselectAllParameters => CurrentParameterSet != null && CurrentParameterSet.Parameters.Any();

		void OnCurrentParameterSet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(CurrentParameterSet.Parameters):
					UpdateCurrentSetsParameters();
					break;
				case nameof(CurrentParameterSet.FilterType):
					FilterModeChanged?.Invoke(this, new FilterTypeChangedArgs(CurrentParameterSet.FilterType));
					break;
				default:
					break;
			}
		}

		private void OnCurrentParameterSet_SelectionChanged(object sender, SelectableParameterSetSelectionChanged e)
		{
			SelectAllParametersCommand.RaiseCanExecuteChanged();
			UnselectAllParametersCommand.RaiseCanExecuteChanged();
			SelectionChanged?.Invoke(this, new SelectableParameterReportFilterSelectionChangedArgs(e.Name, e.ParametersChanged.ToArray()));
		}

		private void UpdateCurrentSetsParameters()
		{
			OnPropertyChanged(nameof(CanSelectAllParameters));
			OnPropertyChanged(nameof(CanDeselectAllParameters));
		}

		public DelegateCommand SwitchToIncludeCommand
		{
			get
			{
				if(_switchToIncludeCommand == null)
				{
					_switchToIncludeCommand = new DelegateCommand(
						() =>
						{
							CurrentParameterSet.FilterType = SelectableFilterType.Include;
						},
						() => CurrentParameterSet != null
					);
					_switchToIncludeCommand.CanExecuteChangedWith(this, x => x.CurrentParameterSet, x => x.HasSelectedSet);
				}
				return _switchToIncludeCommand;
			}
		}

		public DelegateCommand SwitchToExcludeCommand
		{
			get
			{
				if(_switchToExcludeCommand == null)
				{
					_switchToExcludeCommand = new DelegateCommand(
						() =>
						{
							CurrentParameterSet.FilterType = SelectableFilterType.Exclude;

						},
						() => CurrentParameterSet != null
					);
					_switchToExcludeCommand.CanExecuteChangedWith(this, x => x.CurrentParameterSet, x => x.HasSelectedSet);
				}
				return _switchToExcludeCommand;
			}
		}

		public DelegateCommand SelectAllParametersCommand
		{
			get
			{
				if(_selectAllParametersCommand == null)
				{
					_selectAllParametersCommand = new DelegateCommand(
						() =>
						{
							CurrentParameterSet.SelectAll();
						},
						() => CurrentParameterSet != null && CurrentParameterSet.Parameters.Any(x => !x.Selected)
					);
					_selectAllParametersCommand.CanExecuteChangedWith(this, x => x.CurrentParameterSet);
				}
				return _selectAllParametersCommand;
			}
		}

		public DelegateCommand UnselectAllParametersCommand
		{
			get
			{
				if(_unselectAllParametersCommand == null)
				{
					_unselectAllParametersCommand = new DelegateCommand(
						() =>
						{
							CurrentParameterSet.UnselectAll();
						},
						() => CurrentParameterSet != null && CurrentParameterSet.Parameters.Any(x => x.Selected)
					);
					_unselectAllParametersCommand.CanExecuteChangedWith(this, x => x.CurrentParameterSet);
				}
				return _unselectAllParametersCommand;
			}
		}

		public void SilentUpdateSearchValue(string value)
		{
			_searchValue = value;
		}
	}
}
