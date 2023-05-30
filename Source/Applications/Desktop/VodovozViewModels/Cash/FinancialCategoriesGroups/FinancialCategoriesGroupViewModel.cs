using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{

	public class FinancialCategoriesGroupViewModel : EntityTabViewModelBase<FinancialCategoriesGroup>
	{
		private readonly ILifetimeScope _scope;
		private FinancialCategoriesGroup _parentFinancialCategoriesGroup;

		public FinancialCategoriesGroupViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope scope,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_scope = scope ?? throw new System.ArgumentNullException(nameof(scope));

			if(Entity.ParentId.HasValue)
			{
				ParentFinancialCategoriesGroup = UoW.GetById<FinancialCategoriesGroup>(Entity.ParentId.Value);
			}

			var complaintDetalizationEntryViewModelBuilder = new CommonEEVMBuilderFactory<FinancialCategoriesGroupViewModel>(this, this, UoW, NavigationManager, _scope);

			ParentFinancialCategoriesGroupViewModel = complaintDetalizationEntryViewModelBuilder
				.ForProperty(x => x.ParentFinancialCategoriesGroup)
				.UseViewModelDialog<FinancialCategoriesGroupViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.ExcludeFinancialGroupsIds.Add(Entity.Id);
						filter.RestrictNodeTypes.Add(typeof(FinancialCategoriesGroup));
					}
				)
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
