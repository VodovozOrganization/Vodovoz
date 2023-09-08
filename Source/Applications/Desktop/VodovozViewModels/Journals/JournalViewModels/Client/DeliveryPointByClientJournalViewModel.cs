using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
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
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory;

		public DeliveryPointByClientJournalViewModel(
			IDeliveryPointViewModelFactory deliveryPointViewModelFactory,
			DeliveryPointJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			bool hideJournalForOpen,
			bool hideJournalForCreate)
			: base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpen, hideJournalForCreate)
		{
			TabName = "Журнал точек доставки клиента";

			_deliveryPointViewModelFactory =
				deliveryPointViewModelFactory ?? throw new ArgumentNullException(nameof(deliveryPointViewModelFactory));

			UpdateOnChanges(
				typeof(Counterparty),
				typeof(DeliveryPoint)
			);

			SearchEnabled = false;
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<DeliveryPointByClientJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					DeliveryPointByClientJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<DeliveryPointByClientJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					DeliveryPointByClientJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog) {
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None) {
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
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

			if(FilterViewModel?.SearchByAddressViewModel?.SearchValues?.Any() == true)
			{
				foreach(var value in FilterViewModel?.SearchByAddressViewModel?.SearchValues)
				{
					query.Where(Restrictions.InsensitiveLike(
						Projections.Property(() => deliveryPointAlias.CompiledAddress),
						$"%{value}%"
						));
				}
			}

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
			_deliveryPointViewModelFactory.GetForCreationDeliveryPointViewModel(FilterViewModel.Counterparty);

		protected override Func<DeliveryPointByClientJournalNode, DeliveryPointViewModel> OpenDialogFunction => (node) =>
			_deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(node.Id);
	}
}
