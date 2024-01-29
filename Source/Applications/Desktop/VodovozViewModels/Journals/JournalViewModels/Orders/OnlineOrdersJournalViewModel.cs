using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OnlineOrdersJournalViewModel : JournalViewModelBase
	{
		private readonly OnlineOrdersJournalFilterViewModel _filterViewModel;
		private readonly ICommonServices _commonServices;

		public OnlineOrdersJournalViewModel(
			//OnlineOrdersJournalFilterViewModel journalFilterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			Action<OnlineOrdersJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			//_filterViewModel = journalFilterViewModel ?? throw new ArgumentNullException(nameof(journalFilterViewModel));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			
			var dataLoader = new ThreadDataLoader<OnlineOrdersJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			Title = "Журнал заявок на заказы";
			
			ConfigureFilter(filterParams);
			CreateNodeActions();
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultEditAction();
		}

		public IQueryOver<OnlineOrder> ItemsQuery(IUnitOfWork uow)
		{
			OnlineOrder onlineOrderAlias = null;
			Order orderAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			Employee employeeWorkWithAlias = null;
			OnlineOrdersJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => onlineOrderAlias)
				.Left.JoinAlias(o => o.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(o => o.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(o => o.EmployeeWorkWith, () => employeeWorkWithAlias)
				.Left.JoinAlias(o => o.Order, () => orderAlias);

			/*if(_filterViewModel.OnlineOrderStatus.HasValue)
			{
				query.Where(o => o.OnlineOrderStatus == _filterViewModel.OnlineOrderStatus);
			}*/
			
			query.SelectList(list => list
					.SelectGroup(o => o.Id).WithAlias(() => resultAlias.Id)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress)
					.Select(o => o.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(o => o.DeliveryScheduleId).WithAlias(() => resultAlias.DeliveryScheduleId)
					.Select(o => o.OnlineOrderStatus).WithAlias(() => resultAlias.OnlineOrderStatus)
					.Select(() => employeeWorkWithAlias.LastName).WithAlias(() => resultAlias.ManagerWorkWith) // заменить на полноценное имя
					.Select(o => o.Source).WithAlias(() => resultAlias.Source)
					.Select(o => o.OnlineOrderSum).WithAlias(() => resultAlias.OnlineOrderSum)
					.Select(o => o.OnlineOrderPaymentStatus).WithAlias(() => resultAlias.OnlineOrderPaymentStatus)
					.Select(o => o.OnlinePayment).WithAlias(() => resultAlias.OnlinePayment)
					.Select(o => o.OnlineOrderPaymentType).WithAlias(() => resultAlias.OnlineOrderPaymentType)
					.Select(o => o.IsNeedConfirmationByCall).WithAlias(() => resultAlias.IsNeedConfirmationByCall)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				)
				.OrderBy(o => o.OnlineOrderStatus).Asc
				.TransformUsing(Transformers.AliasToBean<OnlineOrdersJournalNode>());

			return query;
		}

		private void ConfigureFilter(Action<OnlineOrdersJournalFilterViewModel> filterParams)
		{
			if(_filterViewModel is null) return;
			filterParams?.Invoke(_filterViewModel);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
		
		private void CreateDefaultEditAction()
		{
			var permissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(OnlineOrder));
			var editAction = new JournalAction("Изменить",
				selected =>
				{
					var selectedNodes = selected.OfType<OnlineOrdersJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					
					return permissionResult.CanRead;
				},
				selected => true,
				selected =>
				{
					var selectedNodes = selected.OfType<OnlineOrdersJournalNode>();
					
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					var selectedNode = selectedNodes.First();
					
					NavigationManager.OpenViewModel<OnlineOrderViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(selectedNode.Id));
				}
			);
			
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}
	}
}
