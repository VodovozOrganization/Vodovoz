using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.JournalViewModels
{
	public class DeliveryPointJournalViewModel : FilterableSingleEntityJournalViewModelBase<DeliveryPoint, DeliveryPointDlg, DeliveryPointJournalNode, DeliveryPointJournalFilterViewModel>
	{
		public DeliveryPointJournalViewModel(DeliveryPointJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) 
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал точек доставки";
			UpdateOnChanges(
				typeof(Counterparty),
				typeof(DeliveryPoint)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultEditAction();
			CreateDeleteAction();
		}

		protected void CreateDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) => {
					var selectedNodes = selected.OfType<DeliveryPointJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					DeliveryPointJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanDelete 
						&& commonServices.CurrentPermissionService.ValidatePresetPermission("can_delete_counterparty_and_deliverypoint");
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<DeliveryPointJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					DeliveryPointJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					if(config.PermissionResult.CanDelete) {
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				}
			);
			NodeActionsList.Add(deleteAction);
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPoint>> ItemsSourceQueryFunction => (uow) => {
			DeliveryPoint deliveryPointAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPointJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<DeliveryPoint>(() => deliveryPointAlias);

			if(FilterViewModel != null && FilterViewModel.RestrictOnlyActive)
				query = query.Where(() => deliveryPointAlias.IsActive);

			if(FilterViewModel != null && FilterViewModel.Counterparty != null)
				query = query.Where(() => counterpartyAlias.Id == FilterViewModel.Counterparty.Id);

			if(FilterViewModel != null && FilterViewModel.RestrictOnlyNotFoundOsm)
				query = query.Where(() => deliveryPointAlias.FoundOnOsm == false);

			if(FilterViewModel != null && FilterViewModel.RestrictOnlyWithoutStreet)
				query = query.Where(Restrictions.Eq
					(
						Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Boolean, "IS_NULL_OR_WHITESPACE(?1)"),
						NHibernateUtil.String, new IProjection[] { Projections.Property(() => deliveryPointAlias.Street) }
					), true
				)
			);

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
				   .Select(() => deliveryPointAlias.FoundOnOsm).WithAlias(() => resultAlias.FoundOnOsm)
				   .Select(() => deliveryPointAlias.IsFixedInOsm).WithAlias(() => resultAlias.FixedInOsm)
				   .Select(() => deliveryPointAlias.IsActive).WithAlias(() => resultAlias.IsActive)
				   .Select(() => deliveryPointAlias.Address1c).WithAlias(() => resultAlias.Address1c)
				   .Select(() => counterpartyAlias.FullName).WithAlias(() => resultAlias.Counterparty)
				)
				.TransformUsing(Transformers.AliasToBean<DeliveryPointJournalNode>());

			return resultQuery;
		};

		protected override Func<DeliveryPointDlg> CreateDialogFunction => () => { throw new NotImplementedException(); };

		protected override Func<DeliveryPointJournalNode, DeliveryPointDlg> OpenDialogFunction => (node) => new DeliveryPointDlg(node.Id);
	}

	public class DeliveryPointJournalNode : JournalEntityNodeBase<DeliveryPoint>
	{
		public string CompiledAddress { get; set; }
		public string LogisticsArea { get; set; }
		public string Address1c { get; set; }
		public string Counterparty { get; set; }
		public bool IsActive { get; set; }
		public bool FoundOnOsm { get; set; }
		public bool FixedInOsm { get; set; }
		public string RowColor { get { return IsActive ? "black" : "grey"; } }
		public string IdString => Id.ToString();
	}
}
