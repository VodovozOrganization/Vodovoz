using QS.Project.Filter;
using QS.Project.Journal;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public class FinancialCategoriesJournalFilterViewModel
		: FilterViewModelBase<FinancialCategoriesJournalFilterViewModel>, IJournalFilterViewModel
	{
		public FinancialCategoriesJournalFilterViewModel()
		{
			ExcludeFinancialGroupsIds.CollectionChanged += OnExcludeIdCollectionChanged;
			RestrictNodeTypes.CollectionChanged += OnRestrictNodeTypesCollectionChanged;
		}

		private void OnRestrictNodeTypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(RestrictNodeTypes));
		}

		private void OnExcludeIdCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(ExcludeFinancialGroupsIds));
		}

		public ObservableCollection<Type> RestrictNodeTypes { get; } = new ObservableCollection<Type>();
		public ObservableCollection<int> ExcludeFinancialGroupsIds { get; } = new ObservableCollection<int>();
		public bool IsShow { get; set; }
	}
}
