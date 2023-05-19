using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Vodovoz.Domain.Cash;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public partial class FinancialCategoriesJournalFilterViewModel
		: FilterViewModelBase<FinancialCategoriesJournalFilterViewModel>, IJournalFilterViewModel
	{
		private readonly Type _financialCategoriesGroupType = typeof(FinancialCategoriesGroup);
		private readonly Type _financialIncomeCategoryType = typeof(IncomeCategory);
		private readonly Type _financialExpenseCategoryType = typeof(ExpenseCategory);
		private readonly Type[] _domainObjectsTypes;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _scope;
		private bool _showArchive;
		private string _titlePart;
		private string _idPart;
		private FinancialCategoriesGroup _parentFinancialGroup;
		private DialogViewModelBase _journalViewModel;
		private Subdivision _subdivision;
		private ExpenseInvoiceDocumentType? _expenseDocumentType;
		private IncomeInvoiceDocumentType? _incomeDocumentType;

		public FinancialCategoriesJournalFilterViewModel(
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			_domainObjectsTypes = new Type[]
			{
				_financialCategoriesGroupType,
				_financialIncomeCategoryType,
				_financialExpenseCategoryType
			};

			SelectableObjectTypes = GetSelectableObjectTypes();

			ExcludeFinancialGroupsIds.CollectionChanged += OnExcludeIdCollectionChanged;
			RestrictNodeTypes.CollectionChanged += OnRestrictNodeTypesCollectionChanged;
			RestrictNodeSelectTypes.CollectionChanged += OnRestrictNodeSelectTypesCollectionChanged;
			_navigationManager = navigationManager;
			_scope = lifetimeScope;
		}

		public bool IsShow { get; set; }

		public ObservableCollection<Type> RestrictNodeTypes { get; } = new ObservableCollection<Type>();

		public ObservableCollection<int> ExcludeFinancialGroupsIds { get; } = new ObservableCollection<int>();

		public ObservableCollection<Type> RestrictNodeSelectTypes { get; } = new ObservableCollection<Type>();

		public List<DomainObjectTypeNode> SelectableObjectTypes { get; }

		public string IdPart
		{
			get => _idPart;
			set => UpdateFilterField(ref _idPart, value);
		}

		public string TitlePart
		{
			get => _titlePart;
			set => UpdateFilterField(ref _titlePart, value);
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

		public ExpenseInvoiceDocumentType? ExpenseDocumentType
		{
			get => _expenseDocumentType;
			set => UpdateFilterField(ref _expenseDocumentType, value);
		}

		public IncomeInvoiceDocumentType? IncomeDocumentType
		{
			get => _incomeDocumentType;
			set => UpdateFilterField(ref _incomeDocumentType, value);
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
						}
					)
					.Finish();

				var subdivisionViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<FinancialCategoriesJournalFilterViewModel>(value, this, UoW, _navigationManager, _scope);

				SubdivisionViewModel = subdivisionViewModelEntryViewModelBuilder
					.ForProperty(x => x.Subdivision)
					.UseViewModelDialog<SubdivisionViewModel>()
					.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
						filter =>
						{
						}
					)
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

		private List<DomainObjectTypeNode> GetSelectableObjectTypes()
		{
			var result = new List<DomainObjectTypeNode>();

			foreach(var domainObjectType in _domainObjectsTypes)
			{
				var node = new DomainObjectTypeNode(domainObjectType, true);
				node.PropertyChanged += SelectableObjectTypesSelectionChanged;
				result.Add(node);
			}

			return result;
		}

		private void SelectableObjectTypesSelectionChanged(object sender, PropertyChangedEventArgs eventArgs)
		{
			if(eventArgs.PropertyName == nameof(DomainObjectTypeNode.Selected))
			{
				Update();
			}
		}
	}
}
