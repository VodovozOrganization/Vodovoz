using Autofac.Core;
using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Vodovoz.Parameters;
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
		private readonly CounterpartyContractFactory _counterpartyContractFactory;
		private readonly ICallTaskWorker _callTaskWorker;

		public RoboatsOrderModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeRepository employeeRepository,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			ICounterpartyContractRepository counterpartyContractRepository,
			CounterpartyContractFactory counterpartyContractFactory,
			ICallTaskWorker callTaskWorker
			)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_orderDailyNumberController = orderDailyNumberController ?? throw new ArgumentNullException(nameof(orderDailyNumberController));
			_paymentFromBankClientController = paymentFromBankClientController ?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
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
				var counterparty = uow.GetById<Counterparty>(roboatsOrderArgs.CounterpartyId);
				var deliveryPoint = uow.GetById<DeliveryPoint>(roboatsOrderArgs.DeliveryPointId);

				Order order = uow.Root;
				order.Client = counterparty;
				order.DeliveryPoint = deliveryPoint;
				order.PaymentType = PaymentType.cash;
				foreach(var waterInfo in roboatsOrderArgs.WatersInfo)
				{
					var nomenclature = uow.GetById<Nomenclature>(waterInfo.NomenclatureId);
					order.AddWaterForSale(nomenclature, waterInfo.BottlesCount);
				}
				order.CalculateDeliveryPrice();
				return order.OrderSum;
			}
		}

		/// <summary>
		/// Создает и подтверждает заказ
		/// </summary>
		/// <param name="roboatsOrderArgs"></param>
		public void CreateAndAcceptOrder(RoboatsOrderArgs roboatsOrderArgs)
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
			}
		}

		/// <summary>
		/// Создает заказ с имеющимися данными в статусе Новый, для обработки его оператором вручную.
		/// </summary>
		public void CreateIncompleteOrder(RoboatsOrderArgs roboatsOrderArgs)
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
				order.SaveEntity(uow, roboatsEmployee, _orderDailyNumberController, _paymentFromBankClientController);
			}
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
					order.PaymentType = PaymentType.cash;
					order.Trifle = roboatsOrderArgs.BanknoteForReturn;
					break;
				case RoboAtsOrderPayment.Terminal:
					order.PaymentType = PaymentType.Terminal;
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
			order.CalculateDeliveryPrice();
			return order;
		}
	}

	public class RoboatsOrderArgs
	{
		public int CounterpartyId { get; set; }
		public int DeliveryPointId { get; set; }
		public IEnumerable<RoboatsWaterInfo> WatersInfo { get; set; }
		public int BottlesReturn { get; set; }
		public DateTime Date { get; set; }
		public int DeliveryScheduleId { get; set; }
		public RoboAtsOrderPayment PaymentType { get; set; }
		public int? BanknoteForReturn { get; set; }
	}

	public enum RoboAtsOrderPayment
	{
		Cash,
		Terminal
	}

	public class RoboatsWaterInfo
	{
		public int NomenclatureId { get; }
		public int BottlesCount { get; }

		public RoboatsWaterInfo(int nomenclatureId, int bottlesCount)
		{
			NomenclatureId = nomenclatureId;
			BottlesCount = bottlesCount;
		}
	}
}
