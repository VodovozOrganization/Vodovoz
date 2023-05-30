using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public partial class FinancialCategoriesJournalFilterViewModel
		: FilterViewModelBase<FinancialCategoriesJournalFilterViewModel>, IJournalFilterViewModel
	{
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _scope;
		private bool _showArchive;
		private string _serachString;
		private FinancialCategoriesGroup _parentFinancialGroup;
		private DialogViewModelBase _journalViewModel;
		private Subdivision _subdivision;
		private TargetDocument? _targetDocument;

		public FinancialCategoriesJournalFilterViewModel(
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			ExcludeFinancialGroupsIds.CollectionChanged += OnExcludeIdCollectionChanged;
			RestrictNodeTypes.CollectionChanged += OnRestrictNodeTypesCollectionChanged;
			RestrictNodeSelectTypes.CollectionChanged += OnRestrictNodeSelectTypesCollectionChanged;
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_scope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public bool IsShow { get; set; }

		public ObservableCollection<Type> RestrictNodeTypes { get; } = new ObservableCollection<Type>();

		public ObservableCollection<int> ExcludeFinancialGroupsIds { get; } = new ObservableCollection<int>();

		public ObservableCollection<Type> RestrictNodeSelectTypes { get; } = new ObservableCollection<Type>();

		public string SearchString
		{
			get => _serachString;
			set => UpdateFilterField(ref _serachString, value);
		}

		public bool ShowArchive
		{
			get => _showArchive;
			set => UpdateFilterField(ref _showArchive, value);
		}

		public FinancialCategoriesGroup ParentFinancialGroup
		{
			get => _parentFinancialGroup;
			set => UpdateFilterField(ref _parentFinancialGroup, value);
		}

		public Subdivision Subdivision
		{
			get => _subdivision;
			set => UpdateFilterField(ref _subdivision, value);
		}

		public TargetDocument? TargetDocument
		{
			get => _targetDocument;
			set => UpdateFilterField(ref _targetDocument, value);
		}

		public IEntityEntryViewModel ParentGroupViewModel { get; private set; }

		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }

		public DialogViewModelBase JournalViewModel
		{
			get => _journalViewModel;
			set
			{
				_journalViewModel = value;

				var financialCategoriesGroupEntryViewModelBuilder = new CommonEEVMBuilderFactory<FinancialCategoriesJournalFilterViewModel>(value, this, UoW, _navigationManager, _scope);

				ParentGroupViewModel = financialCategoriesGroupEntryViewModelBuilder
					.ForProperty(x => x.ParentFinancialGroup)
					.UseViewModelDialog<FinancialCategoriesGroupViewModel>()
					.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
						filter =>
						{
							filter.RestrictNodeTypes.Add(typeof(FinancialCategoriesGroup));
						})
					.Finish();

				var subdivisionViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<FinancialCategoriesJournalFilterViewModel>(value, this, UoW, _navigationManager, _scope);

				SubdivisionViewModel = subdivisionViewModelEntryViewModelBuilder
					.ForProperty(x => x.Subdivision)
					.UseViewModelDialog<SubdivisionViewModel>()
					.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
						filter =>
						{
						})
					.Finish();
			}
		}

		private void OnRestrictNodeTypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Update();
		}

		private void OnExcludeIdCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Update();
		}

		private void OnRestrictNodeSelectTypesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Update();
		}
	}
}
