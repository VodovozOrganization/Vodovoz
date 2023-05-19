using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Domain.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class IncomeCategoryViewModel : EntityTabViewModelBase<IncomeCategory>
	{
		private readonly ILifetimeScope _scope;
		private FinancialCategoriesGroup _parentFinancialCategoriesGroup;

		public IncomeCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IIncomeCategorySelectorFactory incomeCategorySelectorFactory,
			INavigationManager navigationManager,
			ILifetimeScope scope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			IncomeCategoryAutocompleteSelectorFactory =
				(incomeCategorySelectorFactory ?? throw new ArgumentNullException(nameof(incomeCategorySelectorFactory)))
				.CreateDefaultIncomeCategoryAutocompleteSelectorFactory();

			var employeeAutocompleteSelector =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateEmployeeAutocompleteSelectorFactory();

			SubdivisionAutocompleteSelectorFactory =
				(subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory)))
				.CreateDefaultSubdivisionAutocompleteSelectorFactory(employeeAutocompleteSelector);

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			if(Entity.FinancialCategoryGroupId.HasValue)
			{
				ParentFinancialCategoriesGroup = UoW.GetById<FinancialCategoriesGroup>(Entity.FinancialCategoryGroupId.Value);
			}

			var complaintDetalizationEntryViewModelBuilder = new CommonEEVMBuilderFactory<IncomeCategoryViewModel>(this, this, UoW, NavigationManager, _scope);

			ParentFinancialCategoriesGroupViewModel = complaintDetalizationEntryViewModelBuilder
				.ForProperty(x => x.ParentFinancialCategoriesGroup)
				.UseViewModelDialog<FinancialCategoriesGroupViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictNodeTypes.Add(typeof(FinancialCategoriesGroup));
					}
				)
				.Finish();

			TabName = uowBuilder.IsNewEntity ? "Создание новой категории дохода" : $"{Entity.Title}";
		}

		public IEntityAutocompleteSelectorFactory SubdivisionAutocompleteSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory IncomeCategoryAutocompleteSelectorFactory { get; }
		public IEntityEntryViewModel ParentFinancialCategoriesGroupViewModel { get; }

		public FinancialCategoriesGroup ParentFinancialCategoriesGroup
		{
			get => _parentFinancialCategoriesGroup;
			set
			{
				if(SetField(ref _parentFinancialCategoriesGroup, value))
				{
					Entity.FinancialCategoryGroupId = value?.Id;
				}
			}
		}

		public bool IsArchive
		{
			get => Entity.IsArchive;
			set => Entity.SetIsArchiveRecursively(value);
		}
	}
}
