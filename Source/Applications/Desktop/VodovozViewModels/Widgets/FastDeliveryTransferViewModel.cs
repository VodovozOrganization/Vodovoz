using NLog;
using NLog.Fluent;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Parameters;

namespace Vodovoz.ViewModels.Widgets
{
	public class FastDeliveryTransferViewModel : WindowDialogViewModelBase
	{
		private readonly IUnitOfWork _uow;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ICommonServices _commonServices;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private readonly WageParameterService _wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(_parametersProvider));

		private TransferingOrderInfo _transferingOrderInfo;
		private List<RouteList> _routeListsFastDelivery = new List<RouteList>();
		private RouteList _routeListFrom = null;
		private RouteList _routeListTo = null;

		private DelegateCommand _transferCommand;
		private DelegateCommand _cancelCommand;

		public FastDeliveryTransferViewModel(
			//RouteListItem entity,
			//IEntityUoWBuilder uowBuilder,
			//IUnitOfWorkFactory uowFactory,
			INavigationManager navigationManager,
			IRouteListRepository routeListRepository,
			ICommonServices commonServices,
			IRouteListItemRepository routeListItemRepository,
			IRouteListProfitabilityController routeListProfitabilityController
			) : base(navigationManager) //: base(navigationManager)
		{
			_routeListRepository = routeListRepository ?? throw new System.ArgumentNullException(nameof(routeListRepository));
			_commonServices = commonServices ?? throw new System.ArgumentNullException(nameof(commonServices));
			_routeListItemRepository = routeListItemRepository ?? throw new System.ArgumentNullException(nameof(routeListItemRepository));
			_routeListProfitabilityController = routeListProfitabilityController ?? throw new System.ArgumentNullException(nameof(routeListProfitabilityController));

			_uow = UnitOfWorkFactory.CreateWithoutRoot();

			GetRouteListFromInfo(3933326);
		}

		private void GetRouteListFromInfo(int routeListAddressId)
		{
			var transferingRouteListItem = _uow.GetById<RouteListItem>(routeListAddressId);
			
			if (transferingRouteListItem == null || transferingRouteListItem.RouteList == null)
			{
				return;
			}

			_routeListFrom = transferingRouteListItem.RouteList;
		}

		private void TransferAddress(RouteListItem address)
		{
			RouteListItem item = address;
			_logger.Debug("Проверка адреса с номером {0}", item?.Id.ToString() ?? "Неправильный адрес");

			if(item == null || item.Status == RouteListItemStatus.Transfered)
			{
				return;
			}

			//if(!row.IsNeedToReload && !row.IsFromHandToHandTransfer && !row.IsFromFreeBalance)
			//{
			//	transferTypeNotSet.Add(row);
			//	continue;
			//}

			//if(row.IsNeedToReload && routeListTo.Status >= RouteListStatus.EnRoute)
			//{
			//	transferTypeSetAndRlEnRoute.Add(row);
			//	continue;
			//}

			//if(address.IsFromFreeBalance)
			//{
			//	var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(UoW, row.RouteListItem.Order, routeListTo);

			//	if(!hasBalanceForTransfer)
			//	{
			//		deliveryNotEnoughQuantityList.Add(row);
			//		continue;
			//	}
			//}

			var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(_uow, address.Order, _routeListTo);

			if(!hasBalanceForTransfer)
			{
				///Недостаточно свободных остатков
				return;
				//deliveryNotEnoughQuantityList.Add(row);
				//continue;
			}

			if(HasAddressChanges(item))
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Статус {item.Title} был изменён другим пользователем, для его переноса переоткройте диалог.");
				return;
			}

			var transferredAddressFromRouteListTo =
				_routeListItemRepository.GetTransferredRouteListItemFromRouteListForOrder(_uow, _routeListTo.Id, address.Order.Id);

			RouteListItem newItem = null;

			if(transferredAddressFromRouteListTo != null)
			{
				newItem = transferredAddressFromRouteListTo;
				newItem.AddressTransferType = item.AddressTransferType;
				item.WasTransfered = false;
				_routeListTo.RevertTransferAddress(_wageParameterService, newItem, item);
				_routeListFrom.TransferAddressTo(_uow, item, newItem);
				newItem.WasTransfered = true;
			}
			else
			{
				newItem = new RouteListItem(_routeListTo, item.Order, item.Status)
				{
					WasTransfered = true,
					AddressTransferType = AddressTransferType.FromFreeBalance,
					WithForwarder = _routeListTo.Forwarder != null
				};

				_routeListTo.ObservableAddresses.Add(newItem);
				_routeListFrom.TransferAddressTo(_uow, item, newItem);
			}

			if(_routeListTo.Status == RouteListStatus.New)
			{
				if(item.AddressTransferType == AddressTransferType.NeedToReload)
				{
					item.Order.ChangeStatus(OrderStatus.InTravelList);
				}
				if(item.AddressTransferType == AddressTransferType.FromHandToHand)
				{
					item.Order.ChangeStatus(OrderStatus.OnLoading);
				}
			}

			//Пересчёт зарплаты после изменения МЛ
			_routeListFrom.CalculateWages(_wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(_uow, _routeListFrom);
			_routeListTo.CalculateWages(_wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(_uow, _routeListTo);

			item.RecalculateTotalCash();
			newItem.RecalculateTotalCash();

			if(_routeListTo.ClosingFilled)
			{
				newItem.FirstFillClosing(_wageParameterService);
			}

			_uow.Save(item);
			_uow.Save(newItem);

			UpdateTranferDocuments(item, newItem);

			_uow.Commit();
		}

		private void UpdateTranferDocuments(RouteListItem from, RouteListItem to)
		{
			var addressTransferController = new AddressTransferController(new EmployeeRepository());
			addressTransferController.UpdateDocuments(from, to, _uow);
		}

		private bool HasAddressChanges(RouteListItem address)
		{
			RouteListItemStatus actualStatus;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Получение статуса адреса"))
			{
				actualStatus = uow.GetById<RouteListItem>(address.Id).Status;
			}

			if(actualStatus == address.Status)
			{
				return false;
			}

			return true;
		}

		public string AddressInfo => _transferingOrderInfo.Address;
		public string DriverInfo => _transferingOrderInfo.Driver;

		#region Commands

		#region TransferCommand
		public DelegateCommand TransferCommand
		{
			get
			{
				if(_transferCommand == null)
				{
					_transferCommand = new DelegateCommand(Transfer, () => CanTransfer);
					_transferCommand.CanExecuteChangedWith(this, x => x.CanTransfer);
				}
				return _transferCommand;
			}
		}

		public bool CanTransfer => true;

		private void Transfer()
		{

		}

		#endregion

		#region CancelCommand
		public DelegateCommand CancelCommand
		{
			get
			{
				if(_cancelCommand == null)
				{
					_cancelCommand = new DelegateCommand(Cancel, () => CanCancel);
					_cancelCommand.CanExecuteChangedWith(this, x => x.CanCancel);
				}
				return _cancelCommand;
			}
		}

		public bool CanCancel => true;

		private void Cancel()
		{

		}
		#endregion

		#endregion

		private class TransferingOrderInfo
		{
			public string Address { get; set; }
			public string Driver { get; set; }
		}

	}
}
