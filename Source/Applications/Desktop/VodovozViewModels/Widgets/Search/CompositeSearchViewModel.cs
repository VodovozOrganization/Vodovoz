using QS.Commands;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.ViewModels;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

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
		private DelegateCommand _clearSerarchEntriesTextCommand;

		#region IJournalSearch implementation

		public event EventHandler OnSearch;

		public virtual string[] SearchValues
		{
			get => _searchValues ?? new string[] { };
			set => SetField(ref _searchValues, value);
		}

		public void Update()
		{
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

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if(propertyName == nameof(EntrySearchText1)
				|| propertyName == nameof(EntrySearchText2)
				|| propertyName == nameof(EntrySearchText3)
				|| propertyName == nameof(EntrySearchText4))
			{
				SearchValues = new string[] { EntrySearchText1, EntrySearchText2, EntrySearchText3, EntrySearchText4 }
				.Where(x => !string.IsNullOrEmpty(x))
				.ToArray();

				Update();
			}

			base.OnPropertyChanged(propertyName);
		}

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

		#region ClearSerarchEntriesTextCommand

		public DelegateCommand ClearSerarchEntriesTextCommand
		{
			get
			{
				if(_clearSerarchEntriesTextCommand == null)
				{
					_clearSerarchEntriesTextCommand = new DelegateCommand(ClearSerarchEntriesText, () => CanClearSerarchEntriesText);
					_clearSerarchEntriesTextCommand.CanExecuteChangedWith(this, x => x.CanClearSerarchEntriesText);
				}
				return _clearSerarchEntriesTextCommand;
			}
		}

		public bool CanClearSerarchEntriesText => true;

		private void ClearSerarchEntriesText()
		{
			EntrySearchText1 = string.Empty;
			EntrySearchText2 = string.Empty;
			EntrySearchText3 = string.Empty;
			EntrySearchText4 = string.Empty;
		}

		#endregion ClearSerarchEntriesTextCommand

		#endregion Commands
	}
}
