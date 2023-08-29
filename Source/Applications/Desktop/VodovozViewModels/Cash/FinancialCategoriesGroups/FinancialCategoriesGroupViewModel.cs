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

			UpdateInheritedFromParentPropertiesValues(true);
			
			if(!UpdateChildCategoriesAndSubtypes())
			{
				return false;
			}

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
						"Родительская категория папки является архивной. Чтобы изменить параметр, сделайте не архивной родительскую категорию, либо перенесите в не архивную.");
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
						"Родительская категория папки является скрытой. Чтобы изменить параметр, сделайте не скрытой родительскую категорию, либо перенесите в не скрытую.");
				}
			}
		}

		private bool UpdateChildCategoriesAndSubtypes()
		{
			bool isArchivePropertyChanged = _initialIsArchivePropertyValue != Entity.IsArchive;
			bool isHiddenFromPublicAccessPropertyChanged = _initialIsHiddenFromPublicAccessPropertyValue != Entity.IsHiddenFromPublicAccess;
			
			if(Entity.Id == 0
				|| (!isArchivePropertyChanged && !isHiddenFromPublicAccessPropertyChanged))
			{
				return true;
			}

			if(isArchivePropertyChanged)
			{
				if(!Entity.IsArchive
					&& _commonServices.InteractiveService.Question("Снять статус \"В архиве\" у всех вложенных групп и статей?"))
				{
					Entity.SetIsArchivePropertyValueForAllChildItems(UoW, Entity.IsArchive);
				}

				if(Entity.IsArchive)
				{
					if(_commonServices.InteractiveService.Question("Внимание!\r\nCтатус \"В архиве\" будет установлен у всех вложенных групп и статей.\r\nПродолжить?"))
					{
						Entity.SetIsArchivePropertyValueForAllChildItems(UoW, Entity.IsArchive);
					}
					else
					{
						Entity.IsArchive = false;
						return false;
					}
				}
			}

			if(isHiddenFromPublicAccessPropertyChanged)
			{
				if(!Entity.IsHiddenFromPublicAccess
					&& _commonServices.InteractiveService.Question("Снять статус \"Скрыть статью из общего доступа\" у всех вложенных групп и статей?"))
				{
					Entity.SetIsHiddenPropertyValueForAllChildItems(UoW, Entity.IsHiddenFromPublicAccess);
				}

				if(Entity.IsHiddenFromPublicAccess)
				{
					if(_commonServices.InteractiveService.Question("Внимание!\r\nСтатус \"Скрыть статью из общего доступа\" будет установлен у всех вложенных групп и статей.\r\nПродолжить?"))
					{
						Entity.SetIsHiddenPropertyValueForAllChildItems(UoW, Entity.IsHiddenFromPublicAccess);
					}
					else
					{
						Entity.IsHiddenFromPublicAccess = false;
						return false;
					}
				}
			}

			return true;
		}
	}
}
