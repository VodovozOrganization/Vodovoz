using NHibernate.Criterion;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.ViewModels;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.ViewModels.Widgets.Search
{
	public partial class CompositeAlgebraicSearchViewModel : WidgetViewModelBase, IJournalSearch
	{
		private int _searchEntryShownCount = 1;
		private string _entrySearchText1;
		private string _entrySearchText2;
		private string _entrySearchText3;
		private string _entrySearchText4;
		private OperandType _operand1;
		private OperandType _operand2;
		private OperandType _operand3;
		private string[] _searchValues;

		public event EventHandler OnSearch;

		public CompositeAlgebraicSearchViewModel()
		{
			AddSearchEntryCommand = new DelegateCommand(AddSearchEntry, () => CanAddSearchEntry);
			AddSearchEntryCommand.CanExecuteChangedWith(this, x => x.CanAddSearchEntry);

			RemoveSearchEntryCommand = new DelegateCommand(RemoveSearchEntry, () => CanRemoveSearchEntry);
			RemoveSearchEntryCommand.CanExecuteChangedWith(this, x => CanRemoveSearchEntry);

			ClearSearchEntriesTextCommand = new DelegateCommand(ClearSearchEntriesText, () => CanClearSearchEntriesText);
			ClearSearchEntriesTextCommand.CanExecuteChangedWith(this, x => x.CanClearSearchEntriesText);
		}

		#region Properties

		public virtual string[] SearchValues
		{
			get => _searchValues ?? new string[] { };
			set => SetField(ref _searchValues, value);
		}

		public string EntrySearchText1
		{
			get => _entrySearchText1;
			set => SetField(ref _entrySearchText1, value);
		}

		public OperandType Operand1
		{
			get => _operand1;
			set => SetField(ref _operand1, value);
		}

		public string EntrySearchText2
		{
			get => _entrySearchText2;
			set => SetField(ref _entrySearchText2, value);
		}

		public OperandType Operand2
		{
			get => _operand2;
			set => SetField(ref _operand2, value);
		}

		public string EntrySearchText3
		{
			get => _entrySearchText3;
			set => SetField(ref _entrySearchText3, value);
		}

		public OperandType Operand3
		{
			get => _operand3;
			set => SetField(ref _operand3, value);
		}

		public string EntrySearchText4
		{
			get => _entrySearchText4;
			set => SetField(ref _entrySearchText4, value);
		}

		[PropertyChangedAlso(
			nameof(CanAddSearchEntry),
			nameof(CanRemoveSearchEntry))]
		public int SearchEntryShownCount
		{
			get => _searchEntryShownCount;
			set => SetField(ref _searchEntryShownCount, value);
		}

		public bool CanAddSearchEntry => SearchEntryShownCount < 4;

		public bool CanRemoveSearchEntry => SearchEntryShownCount > 1;

		public bool CanClearSearchEntriesText => true;

		public DelegateCommand AddSearchEntryCommand { get; }
		public DelegateCommand RemoveSearchEntryCommand { get; }
		public DelegateCommand ClearSearchEntriesTextCommand { get; }

		#endregion Properties

		public ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			UpdateSearchValues();

			return new CompositeAlgebraicSearchCriterion(this)
				.By(aliasPropertiesExpr)
				.Finish();
		}

		public void Update()
		{
			UpdateSearchValues();
			OnSearch?.Invoke(this, new EventArgs());
		}

		private void UpdateSearchValues()
		{
			SearchValues = new string[] { EntrySearchText1, EntrySearchText2, EntrySearchText3, EntrySearchText4 }
				.Where(x => !string.IsNullOrEmpty(x))
				.ToArray();
		}

		private void AddSearchEntry()
		{
			SearchEntryShownCount++;
			switch(SearchEntryShownCount)
			{
				case 2:
					Operand1 = OperandType.Or;
					break;
				case 3:
					Operand2 = OperandType.Or;
					break;
				case 4:
					Operand3 = OperandType.Or;
					break;
			}
		}

		private void RemoveSearchEntry()
		{
			SearchEntryShownCount--;
			switch(SearchEntryShownCount)
			{
				case 1:
					Operand1 = OperandType.Disabled;
					break;
				case 2:
					Operand2 = OperandType.Disabled;
					break;
				case 3:
					Operand3 = OperandType.Disabled;
					break;
			}
		}

		private void ClearSearchEntriesText()
		{
			EntrySearchText1 = string.Empty;
			EntrySearchText2 = string.Empty;
			EntrySearchText3 = string.Empty;
			EntrySearchText4 = string.Empty;

			Update();
		}
	}
}
