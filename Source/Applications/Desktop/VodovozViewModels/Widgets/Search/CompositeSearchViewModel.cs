using NHibernate.Criterion;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Project.Journal.Search;
using QS.ViewModels;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.ViewModels.Widgets.Search
{
	public class CompositeSearchViewModel : WidgetViewModelBase, IJournalSearch
	{
		private string[] _searchValues;
		private string _searchInfoLabelText = "Поиск";
		private int _searchEntryShownCount = 1;
		private string _entrySearchText1;
		private string _entrySearchText2;
		private string _entrySearchText3;
		private string _entrySearchText4;

		private DelegateCommand _addSearchEntryCommand;
		private DelegateCommand _clearSearchEntriesTextCommand;

		#region IJournalSearch implementation

		public event EventHandler OnSearch;

		public virtual string[] SearchValues
		{
			get => _searchValues ?? new string[] { };
			set => SetField(ref _searchValues, value);
		}

		public void Update()
		{
			SearchValues = new string[] { EntrySearchText1, EntrySearchText2, EntrySearchText3, EntrySearchText4 }
				.Where(x => !string.IsNullOrEmpty(x))
				.ToArray();

			OnSearch?.Invoke(this, new EventArgs());
		}

		#endregion IJournalSearch implementation

		#region Properties

		public string SearchInfoLabelText
		{
			get => _searchInfoLabelText;
			set => SetField(ref _searchInfoLabelText, value);
		}

		[PropertyChangedAlso(nameof(SearchValues))]
		public string EntrySearchText1
		{
			get => _entrySearchText1;
			set => SetField(ref _entrySearchText1, value);
		}

		[PropertyChangedAlso(nameof(SearchValues))]
		public string EntrySearchText2
		{
			get => _entrySearchText2;
			set => SetField(ref _entrySearchText2, value);
		}

		[PropertyChangedAlso(nameof(SearchValues))]
		public string EntrySearchText3
		{
			get => _entrySearchText3;
			set => SetField(ref _entrySearchText3, value);
		}

		[PropertyChangedAlso(nameof(SearchValues))]
		public string EntrySearchText4
		{
			get => _entrySearchText1;
			set => SetField(ref _entrySearchText4, value);
		}

		[PropertyChangedAlso(nameof(CanAddSearchEntry))]
		public int SearchEntryShownCount
		{
			get => _searchEntryShownCount;
			set => SetField(ref _searchEntryShownCount, value);
		}

		#endregion Properties

		public ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr) =>
			new SearchCriterion(this)
				.By(aliasPropertiesExpr)
				.Finish();

		#region Commands

		#region AddSearchEntryCommand

		public DelegateCommand AddSearchEntryCommand
		{
			get
			{
				if(_addSearchEntryCommand == null)
				{
					_addSearchEntryCommand = new DelegateCommand(AddSearchEntry, () => CanAddSearchEntry);
					_addSearchEntryCommand.CanExecuteChangedWith(this, x => x.CanAddSearchEntry);
				}
				return _addSearchEntryCommand;
			}
		}

		public bool CanAddSearchEntry => SearchEntryShownCount < 4;

		private void AddSearchEntry()
		{
			SearchEntryShownCount++;
		}

		#endregion AddSearchEntryCommand

		#region ClearSearchEntriesTextCommand

		public DelegateCommand ClearSearchEntriesTextCommand
		{
			get
			{
				if(_clearSearchEntriesTextCommand == null)
				{
					_clearSearchEntriesTextCommand = new DelegateCommand(ClearSearchEntriesText, () => CanClearSearchEntriesText);
					_clearSearchEntriesTextCommand.CanExecuteChangedWith(this, x => x.CanClearSearchEntriesText);
				}
				return _clearSearchEntriesTextCommand;
			}
		}

		public bool CanClearSearchEntriesText => true;

		private void ClearSearchEntriesText()
		{
			EntrySearchText1 = string.Empty;
			EntrySearchText2 = string.Empty;
			EntrySearchText3 = string.Empty;
			EntrySearchText4 = string.Empty;

			Update();
		}

		#endregion ClearSerarchEntriesTextCommand

		#endregion Commands
	}
}
