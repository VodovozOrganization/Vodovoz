using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Parameters;

namespace Vodovoz.ViewModels.Widgets
{
	public class FastDeliveryTransferViewModel : WidgetViewModelBase
	{
		private readonly IUnitOfWork _uow;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ICommonServices _commonServices;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;

		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly WageParameterService _wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(new ParametersProvider()));

		RouteList _routeListFrom;
		RouteListItem _routeListItemToTransfer;

		private DelegateCommand _transferCommand;
		private DelegateCommand _cancelCommand;

		public FastDeliveryTransferViewModel(
			IRouteListRepository routeListRepository,
			ICommonServices commonServices,
			IRouteListItemRepository routeListItemRepository,
			IRouteListProfitabilityController routeListProfitabilityController,
			int routeListAddressId)
		{
			_routeListRepository = routeListRepository ?? throw new System.ArgumentNullException(nameof(routeListRepository));
			_commonServices = commonServices ?? throw new System.ArgumentNullException(nameof(commonServices));
			_routeListItemRepository = routeListItemRepository ?? throw new System.ArgumentNullException(nameof(routeListItemRepository));
			_routeListProfitabilityController = routeListProfitabilityController ?? throw new System.ArgumentNullException(nameof(routeListProfitabilityController));

			_uow = UnitOfWorkFactory.CreateWithoutRoot();

			GetRouteListFromInfo(routeListAddressId);
		}

		private void GetRouteListFromInfo(int routeListAddressId)
		{
			_routeListItemToTransfer = _uow.GetById<RouteListItem>(routeListAddressId);
			
			if (_routeListItemToTransfer == null || _routeListItemToTransfer.RouteList == null)
			{
				return;
			}

			_routeListFrom = _routeListItemToTransfer.RouteList;
		}

		private void MakeAddressTransfer(RouteListItem address, RouteList routeListFrom, RouteList routeListTo)
		{
			//_logger.Debug("Проверка адреса с номером {0}", address?.Id.ToString() ?? "Неправильный адрес");

			if(address == null
				|| routeListFrom == null
				|| routeListTo == null)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Недостаточно данных для выполнения переноса");
				return;
			}

			if(address.Status == RouteListItemStatus.Transfered)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Данный заказ уже был перенесен");
				return;
			}

			if(routeListTo.AdditionalLoadingDocument == null)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "В выбранном маршрутном листе отсутствуют дополнительная загрузка");
			}

			var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(_uow, address.Order, routeListTo);

			if(!hasBalanceForTransfer)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "В маршрутном листе получателя недостаточно свободных остатков");
				return;
			}

			if(HasAddressChanges(address))
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Статус {address.Title} был изменён другим пользователем, для его переноса переоткройте диалог.");
				return;
			}

			var transferredAddressFromRouteListTo =
				_routeListItemRepository.GetTransferredRouteListItemFromRouteListForOrder(_uow, routeListTo.Id, address.Order.Id);

			if(transferredAddressFromRouteListTo != null)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Данный заказ уже был перенесен в МЛ №{routeListTo.Id}");
				return;
			}

			var newItem = new RouteListItem(routeListTo, address.Order, address.Status)
			{
				WasTransfered = true,
				AddressTransferType = AddressTransferType.FromFreeBalance,
				WithForwarder = routeListTo.Forwarder != null
			};

			routeListTo.ObservableAddresses.Add(newItem);
			routeListFrom.TransferAddressTo(_uow, address, newItem);

			if(routeListTo.Status == RouteListStatus.New)
			{
				if(address.AddressTransferType == AddressTransferType.NeedToReload)
				{
					address.Order.ChangeStatus(OrderStatus.InTravelList);
				}
				if(address.AddressTransferType == AddressTransferType.FromHandToHand)
				{
					address.Order.ChangeStatus(OrderStatus.OnLoading);
				}
			}

			//Пересчёт зарплаты после изменения МЛ
			routeListFrom.CalculateWages(_wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(_uow, routeListFrom);
			routeListTo.CalculateWages(_wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(_uow, routeListTo);

			address.RecalculateTotalCash();
			newItem.RecalculateTotalCash();

			if(routeListTo.ClosingFilled)
			{
				newItem.FirstFillClosing(_wageParameterService);
			}

			_uow.Save(address);
			_uow.Save(newItem);

			UpdateTranferDocuments(address, newItem);

			_uow.Commit();
		}

		private List<RouteListNode> GetFastDeliveryRouteLists()
		{
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			Car carAlias = null;
			RouteListNode routeListNodeAlias = null;

			var routeLists = _uow.Session.QueryOver(() => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => routeListAlias.Car, () => carAlias)
				.Where(() => routeListAlias.AdditionalLoadingDocument != null)
				.Where(rl => routeListAlias.Status == RouteListStatus.EnRoute)
				.SelectList(list => list
					.Select(() => routeListAlias.Id).WithAlias(() => routeListNodeAlias.RouteListId)
					.Select(() => driverAlias.LastName).WithAlias(() => routeListNodeAlias.LastName)
					.Select(() => driverAlias.Name).WithAlias(() => routeListNodeAlias.Name)
					.Select(() => driverAlias.Patronymic).WithAlias(() => routeListNodeAlias.Patronymic)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => routeListNodeAlias.CarRegistrationNumber))
				.TransformUsing(Transformers.AliasToBean<RouteListNode>())
				.List<RouteListNode>()
				.ToList();

			var rowNumber = 0;
			routeLists = routeLists.OrderBy(x => x.LastName).ToList();
			routeLists.ForEach(x => x.RowNumber = ++rowNumber);

			return routeLists;
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

		public RouteListNode RouteListToSelectedNode { get; set; }
		public string AddressInfo => _routeListItemToTransfer?.Order?.DeliveryPoint?.ShortAddress;
		public string DriverInfo => $"от {_routeListFrom?.Driver?.ShortName} {_routeListFrom?.Car?.RegistrationNumber}";
		public List<RouteListNode> RouteListNodes => GetFastDeliveryRouteLists();

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
			if(_routeListFrom == null 
				|| _routeListItemToTransfer == null
				|| RouteListToSelectedNode == null)
			{
				return;
			}

			var routeListTo = _uow.GetById<RouteList>(RouteListToSelectedNode.RouteListId);

			if(routeListTo != null)
			{
				MakeAddressTransfer(_routeListItemToTransfer, _routeListFrom, routeListTo);
			}
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

		public class RouteListNode
		{
			public int RowNumber { get; set; }
			public int RouteListId { get; set; }
			public string CarRegistrationNumber { get; set; } = string.Empty;
			public string Name { get; set; }
			public string LastName { get; set; }
			public string Patronymic { get; set; }
			public string DriverFullName => LastName + " " + Name[0] + "." + (string.IsNullOrWhiteSpace(Patronymic) ? "" : " " + Patronymic[0] + ".");
		}
	}
}
