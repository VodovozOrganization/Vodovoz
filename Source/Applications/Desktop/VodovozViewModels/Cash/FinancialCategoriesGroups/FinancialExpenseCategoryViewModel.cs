using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public class FinancialExpenseCategoryViewModel : EntityTabViewModelBase<FinancialExpenseCategory>
	{
		private readonly ICommonServices _commonServices;
		private readonly ILifetimeScope _scope;
		private FinancialCategoriesGroup _parentFinancialCategoriesGroup;
		private Subdivision _subdivision;

		public FinancialExpenseCategoryViewModel(
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
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			UpdateParentFinancialCategoriesGroup();

			UpdateSubdivision();

			TabName = UoWGeneric.IsNew ? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}" : $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} \"{Entity.Title}\"";

			var parentFinancialCategoriesGroupViewModelBuilder = new CommonEEVMBuilderFactory<FinancialExpenseCategoryViewModel>(this, this, UoW, NavigationManager, _scope);

			ParentFinancialCategoriesGroupViewModel = parentFinancialCategoriesGroupViewModelBuilder
				.ForProperty(x => x.ParentFinancialCategoriesGroup)
				.UseViewModelDialog<FinancialCategoriesGroupViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.ExcludeFinancialGroupsIds.Add(1);
						filter.RestrictFinancialSubtype = FinancialSubType.Expense;
						filter.RestrictNodeTypes.Add(typeof(FinancialCategoriesGroup));
					})
				.Finish();

			var subdivisionViewModelBuilder = new CommonEEVMBuilderFactory<FinancialExpenseCategoryViewModel>(this, this, UoW, NavigationManager, _scope);

			SubdivisionViewModel = subdivisionViewModelBuilder
				.ForProperty(x => x.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(FinancialExpenseCategory.SubdivisionId))
			{
				UpdateSubdivision();
				return;
			}

			if(e.PropertyName == nameof(FinancialExpenseCategory.ParentId))
			{
				UpdateParentFinancialCategoriesGroup();
				UpdateInheritedFromParentPropertiesValues();
				return;
			}

			if(e.PropertyName == nameof(FinancialCategoriesGroup.IsArchive))
			{
				UpdateIsArchivePropertyValue(true);
				return;
			}

			if(e.PropertyName == nameof(FinancialCategoriesGroup.IsHiddenFromPublicAccess))
			{
				UpdateIsHiddenPropertyValue(true);
				return;
			}
		}

		private void UpdateSubdivision()
		{
			if(Entity.SubdivisionId.HasValue)
			{
				Subdivision = UoW.GetById<Subdivision>(Entity.SubdivisionId.Value);
			}
			else
			{
				Subdivision = null;
			}
		}

		private void UpdateParentFinancialCategoriesGroup()
		{
			if(Entity.ParentId.HasValue)
			{
				ParentFinancialCategoriesGroup = UoW.GetById<FinancialCategoriesGroup>(Entity.ParentId.Value);
			}
			else
			{
				ParentFinancialCategoriesGroup = null;
			}
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

			if(Entity.ParentId is null)
			{
				CommonServices.InteractiveService
					.ShowMessage(
						ImportanceLevel.Error,
						$"Необходимо указать родительскую группу",
						"Ошибка");

				return false;
			}

			UpdateInheritedFromParentPropertiesValues(true);
			return result;
		}

		private void UpdateInheritedFromParentPropertiesValues(bool showMessage = false)
		{
			UpdateIsArchivePropertyValue(showMessage);
			UpdateIsHiddenPropertyValue(showMessage);
		}

		private void UpdateIsArchivePropertyValue(bool showMessage = false)
		{
			var parentIsArchive = Entity.IsParentCategoryIsArchive(UoW);

			if(parentIsArchive && !Entity.IsArchive)
			{
				Entity.IsArchive = true;
				if(showMessage)
				{
					_commonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Родительская категория статьи является архивной. Чтобы изменить параметр, сделайте не архивной родительскую категорию, либо перенесите в не архивную.");
				}
			}
		}

		private void UpdateIsHiddenPropertyValue(bool showMessage = false)
		{
			var parentIsHidden = Entity.IsParentCategoryIsHidden(UoW);

			if(parentIsHidden && !Entity.IsHiddenFromPublicAccess)
			{
				Entity.IsHiddenFromPublicAccess = true;
				if(showMessage)
				{
					_commonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Родительская категория статьи является скрытой. Чтобы изменить параметр, сделайте не скрытой родительскую категорию, либо перенесите в не скрытую.");
				}
			}
		}
	}
}
