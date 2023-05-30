using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public class FinancialIncomeCategoryViewModel : EntityTabViewModelBase<FinancialIncomeCategory>
	{
		private readonly ILifetimeScope _scope;
		private FinancialCategoriesGroup _parentFinancialCategoriesGroup;
		private Subdivision _subdivision;

		public FinancialIncomeCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ILifetimeScope scope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(navigation is null)
			{
				throw new ArgumentNullException(nameof(navigation));
			}

			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			if(Entity.ParentId.HasValue)
			{
				ParentFinancialCategoriesGroup = UoW.GetById<FinancialCategoriesGroup>(Entity.ParentId.Value);
			}

			var parentFinancialCategoriesGroupViewModelBuilder = new CommonEEVMBuilderFactory<FinancialIncomeCategoryViewModel>(this, this, UoW, NavigationManager, _scope);

			ParentFinancialCategoriesGroupViewModel = parentFinancialCategoriesGroupViewModelBuilder
				.ForProperty(x => x.ParentFinancialCategoriesGroup)
				.UseViewModelDialog<FinancialCategoriesGroupViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.ExcludeFinancialGroupsIds.Add(2);
						filter.RestrictNodeTypes.Add(typeof(FinancialCategoriesGroup));
					})
				.Finish();

			var subdivisionViewModelBuilder = new CommonEEVMBuilderFactory<FinancialIncomeCategoryViewModel>(this, this, UoW, NavigationManager, _scope);

			SubdivisionViewModel = subdivisionViewModelBuilder
				.ForProperty(x => x.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
					filter =>
					{
					})
				.Finish();
		}

		public FinancialCategoriesGroup ParentFinancialCategoriesGroup
		{
			get => _parentFinancialCategoriesGroup;
			set
			{
				if(SetField(ref _parentFinancialCategoriesGroup, value))
				{
					Entity.ParentId = value?.Id;
				}
			}
		}

		public IEntityEntryViewModel ParentFinancialCategoriesGroupViewModel { get; }

		public Subdivision Subdivision
		{
			get => _subdivision;
			set
			{
				if(SetField(ref _subdivision, value))
				{
					Entity.SubdivisionId = value?.Id;
				}
			}
		}

		public IEntityEntryViewModel SubdivisionViewModel { get; }

		protected override bool BeforeSave()
		{
			var result = base.BeforeSave();

			if(string.IsNullOrWhiteSpace(Entity.Title))
			{
				CommonServices.InteractiveService
					.ShowMessage(
						ImportanceLevel.Error,
						"Нельзя создать группу с пустым именем",
						"Ошибка");
				return false;
			}

			if(!(Entity.Id == 1 || Entity.Id == 2) && Entity.ParentId == null)
			{
				CommonServices.InteractiveService
					.ShowMessage(
						ImportanceLevel.Error,
						$"Необходимо указать родительскую группу",
						"Ошибка");

				return false;
			}

			return result;
		}
	}
}
