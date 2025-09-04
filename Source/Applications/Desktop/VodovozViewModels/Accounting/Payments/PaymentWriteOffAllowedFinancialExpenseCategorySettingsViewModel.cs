using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Database.Common;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.ViewModels.Settings;

namespace Vodovoz.ViewModels.Accounting.Payments
{
	public class PaymentWriteOffAllowedFinancialExpenseCategorySettingsViewModel : NamedDomainEntitiesSettingsViewModelBase
	{
		private const string _parameterName = GeneralSettings.PaymentWriteOffAllowedFinancialExpenseCategoriesParameterName;
		private readonly INavigationManager _navigationManager;
		private readonly DialogViewModelBase _containerViewModel;

		public PaymentWriteOffAllowedFinancialExpenseCategorySettingsViewModel(
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			IGeneralSettings generalSettingsSettings,
			ICurrentPermissionService permissionService,
			DialogViewModelBase containerViewModel)
			: base(commonServices, unitOfWorkFactory, generalSettingsSettings, _parameterName)
		{
			if(permissionService is null)
			{
				throw new ArgumentNullException(nameof(permissionService));
			}

			_navigationManager = navigationManager
				?? throw new ArgumentNullException(nameof(navigationManager));
			_containerViewModel = containerViewModel
				?? throw new ArgumentNullException(nameof(containerViewModel));
			CanEdit = permissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.BookkeeppingPermissions.CanEditPaymentWriteOffAvailableFinancialExpenseCategories);

			Info = "Выбранные статьи могут быть использованы для создания документов списания.";
		}

		protected override void AddEntity()
		{
			Action<FinancialCategoriesJournalFilterViewModel> filterConfig = filter =>
			{
				filter.RestrictFinancialSubtype = FinancialSubType.Expense;
				filter.RestrictNodeSelectTypes.Add(typeof(FinancialExpenseCategory));
			};

			var page = _navigationManager.OpenViewModel<FinancialCategoriesGroupsJournalViewModel, Action<FinancialCategoriesJournalFilterViewModel>>(
				_containerViewModel,
				filterConfig,
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Multiple;
					viewModel.OnSelectResult += OnEntityToAddSelected;
				});
		}

		protected override void GetEntitiesCollection()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				NamedDomainObjectNode resultAlias = null;
				uow.Session.DefaultReadOnly = true;

				var financialExpenseCatogoryIds = GeneralSettingsSettings.PaymentWriteOffAllowedFinancialExpenseCategories;

				var nodes = uow.Session.QueryOver<FinancialExpenseCategory>()
					.WhereRestrictionOn(w => w.Id).IsIn(financialExpenseCatogoryIds)
					.SelectList(list => list
						.Select(w => w.Id).WithAlias(() => resultAlias.Id)
						.Select(w => w.Title).WithAlias(() => resultAlias.Name))
					.TransformUsing(Transformers.AliasToBean<NamedDomainObjectNode>())
					.List<INamedDomainObject>();

				ObservableEntities = new GenericObservableList<INamedDomainObject>(nodes);
			}
		}

		protected override void SaveEntities()
		{
			var ids = ObservableEntities.Select(x => x.Id).ToArray();
			GeneralSettingsSettings
				.UpdatePaymentWriteOffAllowedFinancialExpenseCategoriesParameter(
					ids,
					ParameterName);
			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Данные сохранены");
		}

		private void OnEntityToAddSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.SelectedObjects.OfType<FinancialCategoriesJournalNode>().ToArray();

			if(!selectedNodes.Any())
			{
				return;
			}

			foreach(var selectedNode in selectedNodes)
			{
				var node = ObservableEntities.SingleOrDefault(x => x.Id == selectedNode.Id);

				if(node != null)
				{
					return;
				}

				ObservableEntities.Add(new NamedDomainObjectNode
				{
					Id = selectedNode.Id,
					Name = selectedNode.Name
				});
			}
		}
	}
}
