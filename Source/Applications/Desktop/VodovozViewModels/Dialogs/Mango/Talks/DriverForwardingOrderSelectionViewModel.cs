using Gamma.Utilities;
using Mango.Client;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vodovoz.Application.Mango;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Mango;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.ViewModels.Dialogs.Mango.Talks
{
	/// <summary>
	/// Вью-модель окна выбора заказа клиента для перевода звонка на водителя, доставляющего этот заказ
	/// </summary>
	public class DriverForwardingOrderSelectionViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly IMangoManager _mangoManager;
		private readonly IInteractiveService _interactiveService;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IDriverMangoExtensionNumberRepository _driverMangoExtensionNumberRepository;
		private readonly IUnitOfWork _uow;

		private DriverForwardingOrderNode _selectedOrder;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="counterpartyId">Идентификатор клиента, заказы которого доступны для выбора</param>
		public DriverForwardingOrderSelectionViewModel(
			INavigationManager navigation,
			IUnitOfWorkFactory unitOfWorkFactory,
			IMangoManager mangoManager,
			IInteractiveService interactiveService,
			IOrderRepository orderRepository,
			IRouteListRepository routeListRepository,
			ICounterpartyRepository counterpartyRepository,
			IDriverMangoExtensionNumberRepository driverMangoExtensionNumberRepository,
			int counterpartyId) : base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_mangoManager = mangoManager ?? throw new ArgumentNullException(nameof(mangoManager));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_driverMangoExtensionNumberRepository = driverMangoExtensionNumberRepository ?? throw new ArgumentNullException(nameof(driverMangoExtensionNumberRepository));

			Title = "Перевод звонка на водителя";
			WindowPosition = WindowGravity.None;

			_uow = unitOfWorkFactory.CreateWithoutRoot(nameof(DriverForwardingOrderSelectionViewModel));

			ForwardCallCommand = new DelegateCommand(ForwardCall, () => CanForwardCall);
			ForwardCallCommand.CanExecuteChangedWith(this, x => x.CanForwardCall);

			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));

			Orders = GetOrderNodes(counterpartyId);
		}

		/// <summary>
		/// Команда перевода звонка на водителя выбранного заказа
		/// </summary>
		public DelegateCommand ForwardCallCommand { get; }

		/// <summary>
		/// Команда закрытия окна без перевода звонка
		/// </summary>
		public DelegateCommand CancelCommand { get; }

		/// <summary>
		/// Текущие заказы клиента, на водителей которых можно перевести звонок
		/// </summary>
		public IList<DriverForwardingOrderNode> Orders { get; }

		/// <summary>
		/// Выбранный заказ
		/// </summary>
		[PropertyChangedAlso(nameof(CanForwardCall))]
		public DriverForwardingOrderNode SelectedOrder
		{
			get => _selectedOrder;
			set => SetField(ref _selectedOrder, value);
		}

		/// <summary>
		/// Можно ли перевести звонок на водителя по выбранному заказу
		/// </summary>
		public bool CanForwardCall => SelectedOrder?.CanForwardCall == true;

		private IList<DriverForwardingOrderNode> GetOrderNodes(int counterpartyId)
		{
			var counterparty = _counterpartyRepository.GetCounterpartyById(_uow, counterpartyId);

			if(counterparty == null)
			{
				return new List<DriverForwardingOrderNode>();
			}

			var orders = _orderRepository.GetCurrentOrders(_uow, counterparty)
				.Where(order => !order.SelfDelivery)
				.OrderBy(order => order.DeliveryDate)
				.ToList();

			var driversByOrderId = orders.ToDictionary(
				order => order.Id,
				order => _routeListRepository.GetActualRouteListByOrder(_uow, order)?.Driver);

			var extensionNumbersByDriverId = GetExtensionNumbersByDriverId(driversByOrderId.Values);

			return orders
				.Select(order => CreateOrderNode(order, driversByOrderId[order.Id], extensionNumbersByDriverId))
				.ToList();
		}

		private IDictionary<int, int?> GetExtensionNumbersByDriverId(IEnumerable<Employee> drivers)
		{
			var driverIds = drivers
				.Where(driver => driver != null)
				.Select(driver => driver.Id)
				.Distinct()
				.ToList();

			var extensionNumbers = _driverMangoExtensionNumberRepository
				.GetActiveExtensionNumbersByDriverIdsAsync(_uow, driverIds, CancellationToken.None)
				.GetAwaiter()
				.GetResult();

			return extensionNumbers
				.GroupBy(extensionNumber => extensionNumber.DriverId)
				.ToDictionary(
					group => group.Key,
					group => group.OrderByDescending(extensionNumber => extensionNumber.ActivatedAt).First().ExtensionNumber);
		}

		private DriverForwardingOrderNode CreateOrderNode(
			Order order,
			Employee driver,
			IDictionary<int, int?> extensionNumbersByDriverId)
		{
			int? extensionNumber = null;

			if(driver != null && extensionNumbersByDriverId.TryGetValue(driver.Id, out var driverExtensionNumber))
			{
				extensionNumber = driverExtensionNumber;
			}

			return new DriverForwardingOrderNode
			{
				OrderId = order.Id,
				DeliveryDate = order.DeliveryDate,
				Address = order.DeliveryPoint?.ShortAddress,
				OrderStatusTitle = order.OrderStatus.GetEnumTitle(),
				DriverName = driver?.ShortName,
				DriverExtensionNumber = extensionNumber
			};
		}

		private void ForwardCall()
		{
			if(!CanForwardCall)
			{
				return;
			}

			if(_mangoManager.CurrentTalk == null)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Нет активного разговора, перевести звонок невозможно");

				return;
			}

			var orderNode = SelectedOrder;

			var question = $"Перевести звонок на водителя {orderNode.DriverName} по заказу №{orderNode.OrderId}?";

			if(!_interactiveService.Question(question, Title))
			{
				return;
			}

			_mangoManager.ForwardCall(orderNode.DriverExtensionNumber.Value.ToString(), ForwardingMethod.blind);

			Close(false, CloseSource.Self);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
