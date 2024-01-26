using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Dialogs.Counterparty;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	/// <summary>
	/// Данный журнал главным образом необходим для выбора точки доставки конкретного клиента в различных entityVMentry без колонок с лишней информацией
	/// </summary>
	public class DeliveryPointByClientJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<DeliveryPoint, DeliveryPointViewModel, DeliveryPointByClientJournalNode, DeliveryPointJournalFilterViewModel>
	{
		public DeliveryPointByClientJournalViewModel(
			DeliveryPointJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			bool hideJournalForOpen,
			bool hideJournalForCreate)
			: base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpen, hideJournalForCreate, navigationManager)
		{
			TabName = "Журнал точек доставки клиента";


			UpdateOnChanges(
				typeof(Counterparty),
				typeof(DeliveryPoint)
			);

			SearchEnabled = false;
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<DeliveryPointByClientJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					DeliveryPointByClientJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<DeliveryPointByClientJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					DeliveryPointByClientJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		protected void CreateAddActions()
		{
			if(!EntityConfigs.Any())
			{
				return;
			}

			var totalCreateDialogConfigs = EntityConfigs
				.Where(x => x.Value.PermissionResult.CanCreate)
				.Sum(x => x.Value.EntityDocumentConfigurations
							.Select(y => y.GetCreateEntityDlgConfigs().Count())
							.Sum());

			if(EntityConfigs.Values.Count(x => x.PermissionResult.CanRead) > 1 || totalCreateDialogConfigs > 1)
			{
				var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });
				foreach(var entityConfig in EntityConfigs.Values)
				{
					foreach(var documentConfig in entityConfig.EntityDocumentConfigurations)
					{
						foreach(var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs())
						{
							var childNodeAction = new JournalAction(createDlgConfig.Title,
								(selected) => entityConfig.PermissionResult.CanCreate,
								(selected) => entityConfig.PermissionResult.CanCreate,
								(selected) =>
								{
									createDlgConfig.OpenEntityDialogFunction.Invoke();
									if(documentConfig.JournalParameters.HideJournalForCreateDialog)
									{
										HideJournal(TabParent);
									}
								}
							);
							addParentNodeAction.ChildActionsList.Add(childNodeAction);
						}
					}
				}
				NodeActionsList.Add(addParentNodeAction);
			}
			else
			{
				var entityConfig = EntityConfigs.First().Value;
				var addAction = new JournalAction("Добавить",
					(selected) => entityConfig.PermissionResult.CanCreate,
					(selected) => entityConfig.PermissionResult.CanCreate,
					(selected) =>
					{
						var docConfig = entityConfig.EntityDocumentConfigurations.First();
						docConfig.GetCreateEntityDlgConfigs().First().OpenEntityDialogFunction.Invoke();

						if(docConfig.JournalParameters.HideJournalForCreateDialog)
						{
							HideJournal(TabParent);
						}
					},
					"Insert"
					);
				NodeActionsList.Add(addAction);
			};
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateAddActions();
			CreateEditAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPoint>> ItemsSourceQueryFunction => (uow) =>
		{
			DeliveryPoint deliveryPointAlias = null;
			DeliveryPointByClientJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => deliveryPointAlias);

			if(FilterViewModel?.RestrictOnlyActive == true)
			{
				query.Where(() => deliveryPointAlias.IsActive);
			}

			if(FilterViewModel?.Counterparty != null)
			{
				query.Where(() => deliveryPointAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
			}

			if(FilterViewModel?.RestrictOnlyNotFoundOsm == true)
			{
				query.Where(() => deliveryPointAlias.FoundOnOsm == false);
			}

			if(FilterViewModel?.RestrictOnlyWithoutStreet == true)
			{
				query.Where(() => deliveryPointAlias.Street == null || deliveryPointAlias.Street == " ");
			}

			if(FilterViewModel?.RestrictDeliveryPointId != null)
			{
				query.Where(() => deliveryPointAlias.Id == FilterViewModel.RestrictDeliveryPointId);
			}

			query.Where(FilterViewModel?.SearchByAddressViewModel?.GetSearchCriterion(
				() => deliveryPointAlias.CompiledAddress
			));

			query.Where(GetSearchCriterion(
				() => deliveryPointAlias.Id,
				() => deliveryPointAlias.CompiledAddress
			));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress)
					.Select(() => deliveryPointAlias.IsActive).WithAlias(() => resultAlias.IsActive)
				)
				.TransformUsing(Transformers.AliasToBean<DeliveryPointByClientJournalNode>());

			return resultQuery;
		};

		protected override Func<DeliveryPointViewModel> CreateDialogFunction => () =>
			NavigationManager.OpenViewModel<DeliveryPointViewModel, IEntityUoWBuilder, Counterparty>(this, EntityUoWBuilder.ForCreate(), FilterViewModel.Counterparty).ViewModel;

		protected override Func<DeliveryPointByClientJournalNode, DeliveryPointViewModel> OpenDialogFunction => (node) =>
			NavigationManager.OpenViewModel<DeliveryPointViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel;
	}
}
