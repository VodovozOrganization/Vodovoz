using Autofac;
using FluentNHibernate.Testing.Values;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Cash.FinancialCategoriesGroups
{
	public class FinancialCategoriesGroupViewModel : EntityTabViewModelBase<FinancialCategoriesGroup>
	{
		private readonly ICommonServices _commonServices;
		private readonly ILifetimeScope _scope;
		private FinancialCategoriesGroup _parentFinancialCategoriesGroup;
		private bool _initialIsArchivePropertyValue;
		private bool _initialIsHiddenFromPublicAccessPropertyValue;

		public FinancialCategoriesGroupViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope scope,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_scope = scope ?? throw new System.ArgumentNullException(nameof(scope));

			UpdateParentFinancialCategoriesGroup();

			TabName = UoWGeneric.IsNew ? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}" : $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} \"{Entity.Title}\"";

			var financialCategoriesGroupEntryViewModelBuilder = new CommonEEVMBuilderFactory<FinancialCategoriesGroupViewModel>(this, this, UoW, NavigationManager, _scope);

			ParentFinancialCategoriesGroupViewModel = financialCategoriesGroupEntryViewModelBuilder
				.ForProperty(x => x.ParentFinancialCategoriesGroup)
				.UseViewModelDialog<FinancialCategoriesGroupViewModel>()
				.UseViewModelJournalAndAutocompleter<FinancialCategoriesGroupsJournalViewModel, FinancialCategoriesJournalFilterViewModel>(
					filter =>
					{
						filter.ExcludeFinancialGroupsIds.Add(Entity.Id);
						filter.RestrictNodeTypes.Add(typeof(FinancialCategoriesGroup));
						filter.RestrictFinancialSubtype = ParentFinancialCategoriesGroup?.FinancialSubtype;
					})
				.Finish();

			Entity.PropertyChanged += OnEntityPropertyChanged;

			_initialIsArchivePropertyValue = Entity.IsArchive;
			_initialIsHiddenFromPublicAccessPropertyValue = Entity.IsHiddenFromPublicAccess;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(FinancialCategoriesGroup.ParentId))
			{
				UpdateParentFinancialCategoriesGroup();
				return;
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
					if(value != null)
					{
						Entity.FinancialSubtype = value.FinancialSubtype;
					}
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

			UpdateChildCategoriesAndSubtypes();

			return result;
		}

		private void UpdateChildCategoriesAndSubtypes()
		{
			bool isArchivePropertyChanged = _initialIsArchivePropertyValue != Entity.IsArchive;
			bool isHiddenFromPublicAccessPropertyChanged = _initialIsHiddenFromPublicAccessPropertyValue != Entity.IsHiddenFromPublicAccess;
			
			if(Entity.Id == 0
				|| (!isArchivePropertyChanged && !isHiddenFromPublicAccessPropertyChanged))
			{
				return;
			}

			var childGroups = GetChildCategoryGroups();

			var parentGroupWithChildGroups = new List<FinancialCategoriesGroup>() { Entity };
			parentGroupWithChildGroups.AddRange(childGroups);

			if(Entity.FinancialSubtype == FinancialSubType.Income)
			{
				var childCategories = GetChildIncomeCategories(parentGroupWithChildGroups);

				if(isArchivePropertyChanged)
				{
					if(Entity.IsArchive
						|| _commonServices.InteractiveService.Question("Снять статус \"В архиве\" у всех вложенных групп и статей?"))
					{
						childGroups.ForEach(g => g.IsArchive = Entity.IsArchive);
						childCategories.ForEach(c => c.IsArchive = Entity.IsArchive);
					}
				}

				if(isHiddenFromPublicAccessPropertyChanged)
				{
					if(Entity.IsHiddenFromPublicAccess
						|| _commonServices.InteractiveService.Question("Снять статус \"Скрыть статью из общего доступа\" у всех вложенных групп и статей?"))
					{
						childGroups.ForEach(g => g.IsHiddenFromPublicAccess = Entity.IsHiddenFromPublicAccess);
						childCategories.ForEach(c => c.IsHiddenFromPublicAccess = Entity.IsHiddenFromPublicAccess);
					}
				}
			}
			else if(Entity.FinancialSubtype == FinancialSubType.Expense)
			{
				var childCategories = GetChildExpenseCategories(parentGroupWithChildGroups);

				if(isArchivePropertyChanged)
				{
					if(Entity.IsArchive
						|| _commonServices.InteractiveService.Question("Снять статус \"В архиве\" у всех вложенных групп и статей?"))
					{
						childGroups.ForEach(g => g.IsArchive = Entity.IsArchive);
						childCategories.ForEach(c => c.IsArchive = Entity.IsArchive);
					}
				}

				if(isHiddenFromPublicAccessPropertyChanged)
				{
					if(Entity.IsHiddenFromPublicAccess
						|| _commonServices.InteractiveService.Question("Снять статус \"Скрыть статью из общего доступа\" у всех вложенных групп и статей?"))
					{
						childGroups.ForEach(g => g.IsHiddenFromPublicAccess = Entity.IsHiddenFromPublicAccess);
						childCategories.ForEach(c => c.IsHiddenFromPublicAccess = Entity.IsHiddenFromPublicAccess);
					}
				}
			}
			else
			{
				throw new NotSupportedException("Тип не поддерживается");
			}
		}

		private List<FinancialCategoriesGroup> GetChildCategoryGroups() =>
			Entity.GetAllLevelsSubGroups(UoW, Entity.Id).ToList();

		private List<FinancialIncomeCategory> GetChildIncomeCategories(List<FinancialCategoriesGroup> parentGroups) =>
			Entity.GetFinancialIncomeSubCategories(UoW, parentGroups.Select(g => g.Id)).ToList();

		private List<FinancialExpenseCategory> GetChildExpenseCategories(List<FinancialCategoriesGroup> parentGroups) =>
			Entity.GetFinancialExpenseSubCategories(UoW, parentGroups.Select(g => g.Id)).ToList();
	}
}
