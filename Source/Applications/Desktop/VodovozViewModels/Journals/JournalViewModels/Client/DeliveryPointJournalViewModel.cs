using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	public class DeliveryPointJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<DeliveryPoint, DeliveryPointViewModel, DeliveryPointJournalNode, DeliveryPointJournalFilterViewModel>
	{
		private readonly bool _canDeleteClientAndDp;
		private readonly INavigationManager _navigationManager;

		public DeliveryPointJournalViewModel(
			DeliveryPointJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			bool hideJournalForOpen = false,
			bool hideJournalForCreate = false)
			: base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpen, hideJournalForCreate)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			TabName = "Журнал точек доставки";
			UpdateOnChanges(
				typeof(Counterparty),
				typeof(DeliveryPoint)
			);
			_canDeleteClientAndDp =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_delete_counterparty_and_deliverypoint");

			SearchEnabled = false;
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateEditAction();
			CreateDeleteAction();
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<DeliveryPointJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					DeliveryPointJournalNode selectedNode = selectedNodes.First();
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
					var selectedNodes = selected.OfType<DeliveryPointJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					DeliveryPointJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private void CreateDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<DeliveryPointJournalNode>().ToList();
					if(!selectedNodes.Any())
					{
						return false;
					}

					var selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanDelete && _canDeleteClientAndDp;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<DeliveryPointJournalNode>().ToList();
					if(!selectedNodes.Any())
					{
						return;
					}

					var selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}

					var config = EntityConfigs[selectedNode.EntityType];
					if(config.PermissionResult.CanDelete)
					{
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				},
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPoint>> ItemsSourceQueryFunction => (uow) =>
		{
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPointJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => deliveryPointAlias);

			if(FilterViewModel != null && FilterViewModel.RestrictOnlyActive)
			{
				query.Where(() => deliveryPointAlias.IsActive);
			}

			if(FilterViewModel?.Counterparty != null)
			{
				query.Where(() => counterpartyAlias.Id == FilterViewModel.Counterparty.Id);
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

			if(!string.IsNullOrWhiteSpace(FilterViewModel?.RestrictCounterpartyNameLike))
			{
				query.Where(Restrictions.InsensitiveLike(
					Projections.Property(() => counterpartyAlias.Name),
					$"%{FilterViewModel.RestrictCounterpartyNameLike}%"
					));
			}

			query.Where(FilterViewModel?.SearchByAddressViewModel?.GetSearchCriterion(
				() => deliveryPointAlias.CompiledAddress
			));

			query.Where(GetSearchCriterion(
				() => deliveryPointAlias.Id,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress,
				() => deliveryPointAlias.Address1c
			));

			var resultQuery = query
				.JoinAlias(c => c.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
					.Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress)
					.Select(() => deliveryPointAlias.FoundOnOsm).WithAlias(() => resultAlias.FoundInFias)
					.Select(() => deliveryPointAlias.IsFixedInOsm).WithAlias(() => resultAlias.FixedInFias)
					.Select(() => deliveryPointAlias.IsActive).WithAlias(() => resultAlias.IsActive)
					.Select(() => deliveryPointAlias.Address1c).WithAlias(() => resultAlias.Address1c)
					.Select(() => counterpartyAlias.FullName).WithAlias(() => resultAlias.Counterparty)
				)
				.TransformUsing(Transformers.AliasToBean<DeliveryPointJournalNode>());

			return resultQuery;
		};

		protected override Func<DeliveryPointViewModel> CreateDialogFunction => () => throw new NotImplementedException();

		protected override Func<DeliveryPointJournalNode, DeliveryPointViewModel> OpenDialogFunction => (node) =>
			_navigationManager.OpenViewModel<DeliveryPointViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel;
	}
}
