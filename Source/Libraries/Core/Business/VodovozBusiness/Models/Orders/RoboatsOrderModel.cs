using QS.DomainModel.UoW;
using Sms.Internal;
using System;
using System.Threading.Tasks;
using Vodovoz.Controllers;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Factories;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Models.Orders
{
	public class RoboatsOrderModel
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IOrderDailyNumberController _orderDailyNumberController;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly ICounterpartyContractFactory _counterpartyContractFactory;
		private readonly IFastPaymentSender _fastPaymentSender;
		private readonly ICallTaskWorker _callTaskWorker;

		public RoboatsOrderModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory,
			IFastPaymentSender fastPaymentSender,
			ICallTaskWorker callTaskWorker
			)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderDailyNumberController = orderDailyNumberController ?? throw new ArgumentNullException(nameof(orderDailyNumberController));
			_paymentFromBankClientController = paymentFromBankClientController ?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_fastPaymentSender = fastPaymentSender ?? throw new ArgumentNullException(nameof(fastPaymentSender));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
		}

		/// <summary>
		/// Рассчитывает и возвращает цену заказа по имеющимся данным о заказе
		/// </summary>
		public decimal GetOrderPrice( RoboatsOrderArgs roboatsOrderArgs)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var uow = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);
				if(roboatsEmployee == null)
				{
					throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
				}

				var counterparty = uow.GetById<Counterparty>(roboatsOrderArgs.CounterpartyId);
				var deliveryPoint = uow.GetById<DeliveryPoint>(roboatsOrderArgs.DeliveryPointId);

				Order order = uow.Root;
				order.Author = roboatsEmployee;
				order.Client = counterparty;
				order.DeliveryPoint = deliveryPoint;
				order.PaymentType = PaymentType.Cash;
				foreach(var waterInfo in roboatsOrderArgs.WatersInfo)
				{
					var nomenclature = uow.GetById<Nomenclature>(waterInfo.NomenclatureId);
					order.AddWaterForSale(nomenclature, waterInfo.BottlesCount);
				}
				order.RecalculateItemsPrice();
				order.CalculateDeliveryPrice();
				return order.OrderSum;
			}
		}

		/// <summary>
		/// Создает и подтверждает заказ
		/// Возвращает номер сохраненного заказа
		/// </summary>
		/// <param name="roboatsOrderArgs"></param>
		public int CreateAndAcceptOrder(RoboatsOrderArgs roboatsOrderArgs)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var uow = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);
				if(roboatsEmployee == null)
				{
					throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
				}

				var order = CreateOrder(uow, roboatsEmployee, roboatsOrderArgs);
				order.AcceptOrder(roboatsEmployee, _callTaskWorker);
				order.SaveEntity(uow, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
				return order.Id;
			}
		}

		/// <summary>
		/// Создает заказ с имеющимися данными в статусе Новый, для обработки его оператором вручную.
		/// Возвращает номер сохраненного заказа
		/// </summary>
		public int CreateIncompleteOrder(RoboatsOrderArgs roboatsOrderArgs)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var uow = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var order = CreateIncompleteOrder(uow, roboatsOrderArgs);
				return order.Id;
			}
		}

		private Order CreateIncompleteOrder(IUnitOfWorkGeneric<Order> uow, RoboatsOrderArgs roboatsOrderArgs)
		{
			var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);
			if(roboatsEmployee == null)
			{
				throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
			}

			var order = CreateOrder(uow, roboatsEmployee, roboatsOrderArgs);
			order.SaveEntity(uow, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
			return order;
		}

		/// <summary>
		/// Создает заказ с имеющимися данными в статусе Новый.
		/// Запускает процесс формирования оплаты и отправки QR кода по смс.
		/// Если после 3-х попыток не получилось сформировать оплату, то заказ остается в статусе новый.
		/// Если оплата сформирована то заказ переходит в статус Принят
		/// </summary>
		public async Task<Order> CreateOrderWithPaymentByQrCode(string phone, RoboatsOrderArgs roboatsOrderArgs, bool needAcceptOrder)
		{
			if(roboatsOrderArgs is null)
			{
				throw new ArgumentNullException(nameof(roboatsOrderArgs));
			}

			using(var uowNewOrder = _unitOfWorkFactory.CreateWithNewRoot<Order>())
			{
				var roboatsEmployee = _employeeRepository.GetEmployeeForCurrentUser(uowNewOrder);
				if(roboatsEmployee == null)
				{
					throw new InvalidOperationException("Специальный сотрудник для работы с Roboats должен быть создан и заполнен в параметрах");
				}

				var order = CreateIncompleteOrder(uowNewOrder, roboatsOrderArgs);
				
				if(needAcceptOrder)
				{
					var paymentSended = await TryingSendPayment(phone, order);
					if(paymentSended)
					{
						var acceptedOrder = AcceptOrder(order.Id, roboatsEmployee.Id);
						return acceptedOrder;
					}
				}

				return order;
			}
		}

		private Order AcceptOrder(int orderId, int roboatsEmployee)
		{
			using(var uow = _unitOfWorkFactory.CreateForRoot<Order>(orderId))
			{
				var order = uow.Root;
				var employee = uow.GetById<Employee>(roboatsEmployee);
				order.AcceptOrder(employee, _callTaskWorker);
				order.SaveEntity(uow, employee, _orderDailyNumberController, _paymentFromBankClientController);
				return order;
			}
		}

		private async Task<bool> TryingSendPayment(string phone, Order order)
		{
			FastPaymentResult result;
			int attemptsCount = 0;

			do
			{
				if(attemptsCount > 0)
				{
					await Task.Delay(60000);
				}
				result = await _fastPaymentSender.SendFastPaymentUrlAsync(order, phone, true);

				if(result.Status == ResultStatus.Error && result.OrderAlreadyPaied)
				{
					return true;
				}

				attemptsCount++;

			} while(result.Status == ResultStatus.Error && attemptsCount < 3);

			return result.Status == ResultStatus.Ok;
		}

		private Order CreateOrder(IUnitOfWorkGeneric<Order> uow, Employee author, RoboatsOrderArgs roboatsOrderArgs)
		{
			var counterparty = uow.GetById<Counterparty>(roboatsOrderArgs.CounterpartyId);
			var deliveryPoint = uow.GetById<DeliveryPoint>(roboatsOrderArgs.DeliveryPointId);
			var deliverySchedule = uow.GetById<DeliverySchedule>(roboatsOrderArgs.DeliveryScheduleId);
			Order order = uow.Root;
			order.Author = author;
			order.Client = counterparty;
			order.DeliveryPoint = deliveryPoint;
			switch(roboatsOrderArgs.PaymentType)
			{
				case RoboAtsOrderPayment.Cash:
					order.PaymentType = PaymentType.Cash;
					order.Trifle = roboatsOrderArgs.BanknoteForReturn;
					break;
				case RoboAtsOrderPayment.Terminal:
					order.PaymentType = PaymentType.Terminal;
					break;
				case RoboAtsOrderPayment.QrCode:
					order.PaymentType = PaymentType.DriverApplicationQR;
					order.Trifle = 0;
					break;
				default:
					throw new NotSupportedException("Неподдерживаемый тип оплаты через Roboats");
			}
			order.DeliverySchedule = deliverySchedule;
			order.DeliveryDate = roboatsOrderArgs.Date;

			order.UpdateOrCreateContract(uow, _counterpartyContractRepository, _counterpartyContractFactory);

			foreach(var waterInfo in roboatsOrderArgs.WatersInfo)
			{
				var nomenclature = uow.GetById<Nomenclature>(waterInfo.NomenclatureId);
				order.AddWaterForSale(nomenclature, waterInfo.BottlesCount);
			}
			order.BottlesReturn = roboatsOrderArgs.BottlesReturn;
			order.RecalculateItemsPrice();
			order.CalculateDeliveryPrice();
			order.AddDeliveryPointCommentToOrder();
			return order;
		}
	}
}
