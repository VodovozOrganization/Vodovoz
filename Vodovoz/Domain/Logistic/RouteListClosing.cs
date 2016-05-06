using System;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "закрытия маршрутного листа",
		Nominative = "закрытие маршрутного листа")]
	public class RouteListClosing:PropertyChangedBase, IDomainObject
	{		
		public virtual int Id{ get; set;}

		DateTime closingDate;
		public virtual DateTime ClosingDate{
			get{
				return closingDate;
			}
			set{
				SetField(ref closingDate, value, () => ClosingDate);
			}
		}

		Employee cashier;
		public virtual Employee Cashier{
			get{
				return cashier;
			}
			set{
				SetField(ref cashier, value, () => Cashier);
			}
		}

		RouteList routeList;
		public virtual RouteList RouteList{
			get{
				return routeList;
			}
			set{
				SetField(ref routeList, value, () => RouteList);
			}
		}

		public virtual string Title{
			get{
				return String.Format("Закрытие маршрутного листа №{0}", RouteList.Id);
			}
		}

		public virtual decimal AddressCount
		{
			get{
				return RouteList.Addresses.Count(item => item.IsDelivered());
			}
		}

		public virtual decimal PhoneSum
		{
			get{
				return Wages.GetDriverRates().PhoneServiceCompensationRate * AddressCount;
			}
		}

		public virtual decimal Total
		{
			get{
				return RouteList.Addresses.Sum(address => address.TotalCash + address.DepositsCollected) - PhoneSum;
			}
		}

		public virtual List<BottlesMovementOperation> CreateBottlesMovementOperation(){
			var result = new List<BottlesMovementOperation>();
			foreach (RouteListItem address in RouteList.Addresses)
			{
				int amountDelivered= address.Order.OrderItems
					.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
					.Sum(item => item.ActualCount);
				if (amountDelivered != 0 || address.BottlesReturned != 0)
				{
					var bottlesMovementOperation = new BottlesMovementOperation
					{
						OperationTime = DateTime.Now,
						Order = address.Order,
						Delivered = amountDelivered,
						Returned = address.BottlesReturned,
						Counterparty = address.Order.Client,
						DeliveryPoint = address.Order.DeliveryPoint
					};
					address.Order.BottlesMovementOperation = bottlesMovementOperation;
					result.Add(bottlesMovementOperation);
				}
			}
			return result;
		}

		public virtual List<CounterpartyMovementOperation> CreateCounterpartyMovementOperations(){
			var result = new List<CounterpartyMovementOperation>();
			foreach (var orderItem in RouteList.Addresses.SelectMany(item=>item.Order.OrderItems)
				.Where(item=>Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
				.Where(item=>!item.Nomenclature.Serial)
			)
			{
				var amount = (orderItem.ActualCount);
				if (amount > 0)
				{
					var counterpartyMovementOperation = new CounterpartyMovementOperation
						{
							OperationTime = DateTime.Now,
							Amount = amount,
							Nomenclature = orderItem.Nomenclature,
							Equipment = orderItem.Equipment,
							IncomingCounterparty = orderItem.Order.Client,
							IncomingDeliveryPoint = orderItem.Order.DeliveryPoint
						};
					result.Add(counterpartyMovementOperation);
				}
			}

			//Проверка на время тестирования, с более понятным сообщением что прозошло. Если отладим процес можно будет убрать.
			if (RouteList.Addresses.SelectMany(item => item.Order.OrderEquipments).Any(item => item.Equipment == null))
				throw new InvalidOperationException("В заказе присутстует оборудование без указания серийного номера. К моменту закрытия такого быть не должно.");

			foreach (var orderEquipment in RouteList.Addresses.SelectMany(item=>item.Order.OrderEquipments)
				.Where(item=>Nomenclature.GetCategoriesForShipment().Contains(item.Equipment.Nomenclature.Category)))
			{
				var amount = orderEquipment.Confirmed ? 1 : 0;
				if (amount > 0)
				{
					if (orderEquipment.Direction == Direction.Deliver)
					{
						var counterpartyMovementOperation = new CounterpartyMovementOperation
						{
							OperationTime = DateTime.Now,
							Amount = amount,
							Nomenclature = orderEquipment.Equipment.Nomenclature,
							Equipment = orderEquipment.Equipment,
							ForRent = orderEquipment.Reason == Reason.Rent,
							IncomingCounterparty = orderEquipment.Order.Client,
							IncomingDeliveryPoint = orderEquipment.Order.DeliveryPoint
						};
						result.Add(counterpartyMovementOperation);
					}
					else
					{
						var counterpartyMovementOperation = new CounterpartyMovementOperation
							{
								OperationTime = DateTime.Now,
								Amount = amount,
								Nomenclature = orderEquipment.Equipment.Nomenclature,
								Equipment = orderEquipment.Equipment,
								WriteoffCounterparty = orderEquipment.Order.Client,
								WriteoffDeliveryPoint = orderEquipment.Order.DeliveryPoint
							};
						result.Add(counterpartyMovementOperation);
					}
				}
			}
			return result;
		}

		public virtual List<DepositOperation> CreateDepositOperations(IUnitOfWork UoW){
			var result = new List<DepositOperation>();
			var bottleDepositNomenclature = NomenclatureRepository.GetBottleDeposit(UoW);
			var bottleDepositPrice = bottleDepositNomenclature.GetPrice(1);
			foreach (RouteListItem item in RouteList.Addresses.Where(address=>address.Order.PaymentType==PaymentType.cash))
			{
				var deliveredEquipmentForRent = item.Order.OrderEquipments.Where(eq => eq.Confirmed)
						.Where(eq => eq.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
						.Where(eq => eq.Reason == Reason.Rent);

				var paidRentDepositsFromClient = item.Order.OrderDepositItems
						.Where(deposit => deposit.PaymentDirection == PaymentDirection.FromClient)
						.Where(deposit => deposit.PaidRentItem != null
					                       && deliveredEquipmentForRent.Any(eq => eq.Id == deposit.PaidRentItem.Equipment.Id));

				var freeRentDepositsFromClient = item.Order.OrderDepositItems
						.Where(deposit => deposit.PaymentDirection == PaymentDirection.FromClient)
						.Where(deposit => deposit.FreeRentItem != null
					                       && deliveredEquipmentForRent.Any(eq => eq.Id == deposit.FreeRentItem.Equipment.Id));

				foreach (var deposit in paidRentDepositsFromClient.Union(freeRentDepositsFromClient))
				{
					var operation = new DepositOperation
					{
						Order = item.Order,
						OperationTime = DateTime.Now,
						DepositType = DepositType.Equipment,
						Counterparty = item.Order.Client,
						DeliveryPoint = item.Order.DeliveryPoint,
						ReceivedDeposit = deposit.Total			
					};
					deposit.DepositOperation = operation;
					result.Add(operation);
				}

				var pickedUpEquipmentForRent = item.Order.OrderEquipments.Where(eq => eq.Confirmed)
					.Where(eq => eq.Direction == Vodovoz.Domain.Orders.Direction.PickUp)
					.Where(eq => eq.Reason == Reason.Rent);

				var paidRentDepositsToClient = item.Order.OrderDepositItems
					.Where(deposit => deposit.PaymentDirection == PaymentDirection.ToClient)
					.Where(deposit => deposit.PaidRentItem != null
						&& pickedUpEquipmentForRent.Any(eq => eq.Id == deposit.PaidRentItem.Equipment.Id));

				var freeRentDepositsToClient = item.Order.OrderDepositItems
					.Where(deposit => deposit.PaymentDirection == PaymentDirection.ToClient)
					.Where(deposit => deposit.FreeRentItem != null
						&& pickedUpEquipmentForRent.Any(eq => eq.Id == deposit.FreeRentItem.Equipment.Id));
				
				foreach (var deposit in paidRentDepositsToClient.Union(freeRentDepositsToClient))
				{
					var operation = new DepositOperation
						{
							Order = item.Order,
							OperationTime = DateTime.Now,
							DepositType = DepositType.Equipment,
							Counterparty = item.Order.Client,
							DeliveryPoint = item.Order.DeliveryPoint,
							RefundDeposit = deposit.Total			
						};
					deposit.DepositOperation = operation;
					result.Add(operation);
				}					

				var bottleDepositsOperation = new DepositOperation()
				{
					Order = item.Order,
					OperationTime = DateTime.Now,
					DepositType = DepositType.Bottles,
					Counterparty = item.Order.Client,
					DeliveryPoint = item.Order.DeliveryPoint,
					ReceivedDeposit = item.DepositsCollected>0 ? item.DepositsCollected : 0,
					RefundDeposit = item.DepositsCollected<0 ? -item.DepositsCollected : 0
				};					
				
				var depositsCount = (int)(Math.Abs(item.DepositsCollected) / bottleDepositPrice);
				var depositOrderItem = item.Order.ObservableOrderItems.FirstOrDefault (i => i.Nomenclature.Id == bottleDepositNomenclature.Id);
				var depositItem = item.Order.ObservableOrderDepositItems.FirstOrDefault (i => i.DepositType == DepositType.Bottles);

				if (item.DepositsCollected>0) {
					if (depositItem != null) {
						depositItem.Deposit = bottleDepositPrice;
						depositItem.Count = depositsCount;
						depositItem.PaymentDirection = PaymentDirection.FromClient;
						depositItem.DepositOperation = bottleDepositsOperation;
					}
					if (depositOrderItem != null)
					{
						depositOrderItem.Count = depositsCount;
						depositOrderItem.ActualCount = depositsCount;
					}
					else {
						item.Order.ObservableOrderItems.Add (new OrderItem {
							Order = item.Order,
							AdditionalAgreement = null,
							Count = depositsCount,
							ActualCount = depositsCount,
							Equipment = null,
							Nomenclature = bottleDepositNomenclature,
							Price = bottleDepositPrice
						});
						item.Order.ObservableOrderDepositItems.Add (new OrderDepositItem {
							Order = item.Order,
							Count = depositsCount,
							Deposit = bottleDepositPrice,
							DepositOperation = bottleDepositsOperation,
							DepositType = DepositType.Bottles,
							FreeRentItem = null,
							PaidRentItem = null,
							PaymentDirection = PaymentDirection.FromClient
						});
					}
				}
				if (item.DepositsCollected==0) {
					if (depositItem != null)
						item.Order.ObservableOrderDepositItems.Remove (depositItem);
					if (depositOrderItem != null)
						item.Order.ObservableOrderItems.Remove (depositOrderItem);					
				}
				if (item.DepositsCollected<0) {
					if (depositOrderItem != null)
						item.Order.ObservableOrderItems.Remove (depositOrderItem);
					if (depositItem != null) {
						depositItem.Deposit = bottleDepositPrice;
						depositItem.Count = depositsCount;
						depositItem.PaymentDirection = PaymentDirection.ToClient;
						depositItem.DepositOperation = bottleDepositsOperation;
					} else
						item.Order.ObservableOrderDepositItems.Add (new OrderDepositItem {
							Order = item.Order,
							DepositOperation = bottleDepositsOperation,
							DepositType = DepositType.Bottles,
							Deposit = bottleDepositPrice,
							PaidRentItem = null,
							FreeRentItem = null,
							PaymentDirection = PaymentDirection.ToClient,
							Count = depositsCount
						});
				}
				if(bottleDepositsOperation.RefundDeposit!=0 || bottleDepositsOperation.ReceivedDeposit!=0)
					result.Add(bottleDepositsOperation);
			}
			return result;
		}

		public virtual List<MoneyMovementOperation> CreateMoneyMovementOperations(IUnitOfWork uow, ref Income cashIncome, ref Expense cashExpense)
		{
			var result = new List<MoneyMovementOperation>();
			foreach (var address in routeList.Addresses)
			{
				var order = address.Order;
				var depositsTotal = order.OrderDepositItems.Sum(dep => dep.Count * dep.Deposit);
				Decimal? money = null;
				if (order.PaymentType == PaymentType.cash)
					money = address.TotalCash;
				var moneyMovementOperation = new MoneyMovementOperation()
				{
					OperationTime = DateTime.Now,
					Order = order,
					Counterparty = order.Client,
					PaymentType = order.PaymentType,
					Debt = order.ActualGoodsTotalSum,
					Money = money,
					Deposit = depositsTotal
				};				
				order.MoneyMovementOperation = moneyMovementOperation;
				result.Add(moneyMovementOperation);
			}
			if (Total > 0)
			{
				cashIncome = new Income
				{
					IncomeCategory = Repository.Cash.CategoryRepository.RouteListClosingIncomeCategory(uow),
					TypeOperation = IncomeType.DriverReport,
					Date = DateTime.Now,
					Casher = cashier,
					Employee = RouteList.Driver,
					Description =$"Закрытие МЛ #{RouteList.Id}",
					Money = Total,
				};
				cashIncome.RouteListClosing = this;
			}
			else
			{
				cashExpense = new Expense
					{
						ExpenseCategory = Repository.Cash.CategoryRepository.RouteListClosingExpenseCategory(uow),
						TypeOperation = ExpenseType.Expense,
						Date = DateTime.Now,
						Casher = cashier,
						Employee = RouteList.Driver,
						Description =$"Закрытие МЛ #{RouteList.Id}",
						Money = -Total,
					};
				cashExpense.RouteListClosing = this;
			}
			return result;
		}

		public virtual void Confirm()
		{
			if (RouteList.Status != RouteListStatus.ReadyToReport)
				throw new InvalidOperationException(String.Format("Закрыть маршрутный лист можно только если он находится в статусе {0}", RouteListStatus.ReadyToReport));
			
			RouteList.Status = RouteListStatus.MileageCheck;
			foreach (var address in RouteList.Addresses)
			{
				if(address.Order.OrderStatus == OrderStatus.Shipped || address.Order.OrderStatus == OrderStatus.OnTheWay)
				{
					address.Order.OrderStatus = OrderStatus.Closed;
					address.UpdateStatus(RouteListItemStatus.Completed);
				}
				if (address.Status == RouteListItemStatus.Canceled)
					address.Order.ChangeStatus(OrderStatus.DeliveryCanceled);
				if(address.Status == RouteListItemStatus.Overdue)
					address.Order.ChangeStatus(OrderStatus.NotDelivered);
			}
			ClosingDate = DateTime.Now;
		}

		public virtual void RequestAdditionalUnloading()
		{
			RouteList.Status = RouteListStatus.EnRoute;
		}
	}
}

