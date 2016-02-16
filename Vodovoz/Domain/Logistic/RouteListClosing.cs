using System;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

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

		public virtual List<DepositOperation> CreateDepositOperations(){
			var result = new List<DepositOperation>();
			foreach (RouteListItem item in RouteList.Addresses)
			{
				if (item.Order.PaymentType == PaymentType.cash)
				{
					var deliveredEquipmentForRent = item.Order.OrderEquipments.Where(eq => eq.Confirmed)
						.Where(eq => eq.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
						.Where(eq => eq.Reason == Reason.Rent);

					var paidRentDeposits = item.Order.OrderDepositItems
						.Where(deposit => deposit.PaymentDirection == PaymentDirection.FromClient)
						.Where(deposit => deposit.PaidRentItem != null
						                       && deliveredEquipmentForRent.Any(eq => eq.Id == deposit.PaidRentItem.Equipment.Id));

					var freeRentDeposits = item.Order.OrderDepositItems
						.Where(deposit => deposit.PaymentDirection == PaymentDirection.FromClient)
						.Where(deposit => deposit.FreeRentItem != null
						                       && deliveredEquipmentForRent.Any(eq => eq.Id == deposit.FreeRentItem.Equipment.Id));

					foreach (var deposit in paidRentDeposits.Union(freeRentDeposits))
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

					var bottleDepositsOperation = new DepositOperation
						{
							Order = item.Order,
							OperationTime = DateTime.Now,
							DepositType = DepositType.Equipment,
							Counterparty = item.Order.Client,
							DeliveryPoint = item.Order.DeliveryPoint,
							ReceivedDeposit = item.DepositsCollected
						};
					result.Add(bottleDepositsOperation);
				}
			}
			return result;
		}

		public virtual void Confirm()
		{			
			RouteList.Status = RouteListStatus.MileageCheck;
			foreach (var order in RouteList.Addresses.Select(item=>item.Order))
			{
				order.OrderStatus = OrderStatus.Closed;
			}
			ClosingDate = DateTime.Now;
		}
	}
}

