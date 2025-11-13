using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using static Vodovoz.ViewModels.Counterparties.ClientEquipmentBalanceJournalViewModel;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class ClientEquipmentBalanceJournalViewModel : EntityJournalViewModelBase<CounterpartyMovementOperation, DialogViewModelBase, ClientEquipmentBalanceNode>
	{
		private readonly ClientBalanceFilterViewModel _clientBalanceFilterViewModel;

		public ClientEquipmentBalanceJournalViewModel(
			ClientBalanceFilterViewModel clientBalanceFilterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			IUserService userService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_clientBalanceFilterViewModel = clientBalanceFilterViewModel
				?? throw new System.ArgumentNullException(nameof(clientBalanceFilterViewModel));

			TabName = "Оборудование у клиентов";

			_clientBalanceFilterViewModel.Journal = this;

			_clientBalanceFilterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _clientBalanceFilterViewModel;

			UserHaveAccessOnlyToWarehouseAndComplaints =
				currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
					&& !userService.GetCurrentUser().IsAdmin;

			_clientBalanceFilterViewModel.CanChangeDeliveryPoint = !UserHaveAccessOnlyToWarehouseAndComplaints;

			NodeActionsList.Clear();
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public bool UserHaveAccessOnlyToWarehouseAndComplaints { get; }

		protected override IQueryOver<CounterpartyMovementOperation> ItemsQuery(IUnitOfWork unitOfWork)
		{
			Nomenclature nomenclatureAlias = null;
			ClientEquipmentBalanceNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Equipment equipmentAlias = null;
			CounterpartyMovementOperation operationAlias = null;
			CounterpartyMovementOperation subsequentOperationAlias = null;

			var lastCouterpartyOp = unitOfWork.Session.QueryOver(() => operationAlias)
				.JoinAlias(o => o.Equipment, () => equipmentAlias)
				.Where(o => o.Equipment != null);

			if(_clientBalanceFilterViewModel.RestrictIncludeSold == false)
			{
				lastCouterpartyOp.Where(x => x.ForRent == true);
			}

			if(_clientBalanceFilterViewModel.DeliveryPoint == null)
			{
				if(_clientBalanceFilterViewModel.RestrictCounterparty != null)
				{
					lastCouterpartyOp.Where(x => x.IncomingCounterparty == _clientBalanceFilterViewModel.RestrictCounterparty);
				}
				else
				{
					lastCouterpartyOp.Where(x => x.IncomingCounterparty != null);
				}
			}
			else
			{
				lastCouterpartyOp.Where(x => x.IncomingDeliveryPoint == _clientBalanceFilterViewModel.DeliveryPoint);
			}

			if(_clientBalanceFilterViewModel.Nomenclature != null)
			{
				lastCouterpartyOp.Where(() => equipmentAlias.Nomenclature.Id == _clientBalanceFilterViewModel.Nomenclature.Id);
			}

			var subsequentOperationsSubquery = QueryOver.Of(() => subsequentOperationAlias)
				.Where(() => operationAlias.OperationTime < subsequentOperationAlias.OperationTime && operationAlias.Equipment == subsequentOperationAlias.Equipment)
				.Select(op => op.Id);

			lastCouterpartyOp.WithSubquery.WhereNotExists(subsequentOperationsSubquery);

			return lastCouterpartyOp
				.JoinAlias(o => o.IncomingCounterparty, () => counterpartyAlias)
				.JoinAlias(o => o.IncomingDeliveryPoint, () => deliveryPointAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.SelectList(list => list
					.Select(() => equipmentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => counterpartyAlias.FullName).WithAlias(() => resultAlias.Client)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.Address)
					.Select(() => operationAlias.ForRent).WithAlias(() => resultAlias.IsOur)
					.Select(() => equipmentAlias.Id).WithAlias(() => resultAlias.SerialNumberInt))
				.OrderBy(x => x.OperationTime).Desc
				.TransformUsing(Transformers.AliasToBean<ClientEquipmentBalanceNode>());
		}

		public override void Dispose()
		{
			_clientBalanceFilterViewModel.OnFiltered -= OnFilterViewModelFiltered;

			base.Dispose();
		}
	}
}
