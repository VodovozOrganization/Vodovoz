using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository
{
	public static class OrderRepository
	{
		public static Dictionary<KeyToDocumentsSet, OrderDocumentType[]> GetAllRulesForDocumetsCollecting(){
			Dictionary<KeyToDocumentsSet, OrderDocumentType[]> rulesForOrderDocumentsColecting
																= new Dictionary<KeyToDocumentsSet, OrderDocumentType[]>();
			//rule#1.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					HasOrderItems = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);
			//rule#1.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					HasOrderItems = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter
				}
			);
			//rule#1.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					HasOrderItems = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);
			//rule#1.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					HasOrderItems = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.DriverTicket,
					OrderDocumentType.UPD,
					OrderDocumentType.Bill
				}
			);
			//rule#1.3.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					DefaultDocumentType = DefaultDocumentType.torg12,
					PaymentType = PaymentType.cashless,
					HasOrderItems = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.DriverTicket,
					OrderDocumentType.UPD,
					OrderDocumentType.Bill,
					OrderDocumentType.Torg12,
					OrderDocumentType.ShetFactura
				}
			);
			//реализовать самовывоз
			////rule#1.3.2
			//rulesForOrderDocumentsColecting.Add(
			//	new KeyToDocumentsSet {
			//		PaymentType = PaymentType.cashless,
			//		HasOrderItems = true
			//	},
			//	new OrderDocumentType[] {
			//		OrderDocumentType.DriverTicket,
			//		OrderDocumentType.UPD,
			//		OrderDocumentType.Bill
			//	}
			//);
			////rule#1.3.3
			//rulesForOrderDocumentsColecting.Add(
			//	new KeyToDocumentsSet {
			//		DefaultDocumentType = DefaultDocumentType.torg12,
			//		PaymentType = PaymentType.cashless,
			//		HasOrderItems = true
			//	},
			//	new OrderDocumentType[] {
			//		OrderDocumentType.DriverTicket,
			//		OrderDocumentType.UPD,
			//		OrderDocumentType.Bill,
			//		OrderDocumentType.Torg12,
			//		OrderDocumentType.ShetFactura
			//	}
			//);
			//rule#1.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					HasOrderItems = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);

			//rule#2.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					HasOrderItems = true,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#2.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					HasOrderItems = true,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#2.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					HasOrderItems = true,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#2.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					HasOrderItems = true,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.DriverTicket,
					OrderDocumentType.UPD,
					OrderDocumentType.Bill,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#2.3.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					DefaultDocumentType = DefaultDocumentType.torg12,
					PaymentType = PaymentType.cashless,
					HasOrderItems = true,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.DriverTicket,
					OrderDocumentType.UPD,
					OrderDocumentType.Bill,
					OrderDocumentType.EquipmentTransfer,
					OrderDocumentType.Torg12,
					OrderDocumentType.ShetFactura
				}
			);
			//rule#2.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					HasOrderItems = true,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice,
					OrderDocumentType.EquipmentTransfer
				}
			);

			//rule#3.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					HasOrderItems = true,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#3.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					HasOrderItems = true,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#3.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					HasOrderItems = true,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#3.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					HasOrderItems = true,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#3.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					HasOrderItems = true,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#4.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					HasOrderItems = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);
			//rule#4.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					HasOrderItems = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter
				}
			);
			//rule#4.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					HasOrderItems = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);
			//rule#4.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					HasOrderItems = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#4.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					HasOrderItems = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#5.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#5.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#5.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] { }
			);
			//rule#5.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] { }
			);
			//rule#5.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					HasOrderEquipment = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#6.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#6.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter,
					OrderDocumentType.EquipmentTransfer
				}
			);
			//rule#6.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#6.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#6.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					HasOrderEquipment = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#7.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);
			//rule#7.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter
				}
			);
			//rule#7.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#7.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#7.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					NeedToRefundDepositFromClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#8.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					NeedToReturnBottles = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);
			//rule#8.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					NeedToReturnBottles = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter
				}
			);
			//rule#8.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					NeedToReturnBottles = true
				},
				new OrderDocumentType[] { }
			);
			//rule#8.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					NeedToReturnBottles = true
				},
				new OrderDocumentType[] { }
			);
			//rule#8.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					NeedToReturnBottles = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);




















			return rulesForOrderDocumentsColecting;
		}

		public static ListStore GetListStoreSumDifferenceReasons (IUnitOfWork uow)
		{
			Vodovoz.Domain.Orders.Order order = null;

			var reasons = uow.Session.QueryOver<VodovozOrder> (() => order)
				.Select (Projections.Distinct (Projections.Property (() => order.SumDifferenceReason)))
				.List<string> ();

			var store = new ListStore (typeof(string));
			foreach (string s in reasons) {
				store.AppendValues (s);
			}
			return store;
		}
			
		public static QueryOver<VodovozOrder> GetOrdersForRLEditingQuery (DateTime date, bool showShipped)
		{
			var query = QueryOver.Of<VodovozOrder>();
			if (!showShipped)
				query.Where(order => order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList);
			else
				query.Where(order => order.OrderStatus != OrderStatus.Canceled && order.OrderStatus != OrderStatus.NewOrder && order.OrderStatus != OrderStatus.WaitForPayment);
			return query.Where(order => order.DeliveryDate == date.Date && !order.SelfDelivery);
		}

		public static IList<VodovozOrder> GetAcceptedOrdersForRegion (IUnitOfWork uow, DateTime date, LogisticsArea area)
		{
			DeliveryPoint point = null;
			return uow.Session.QueryOver<VodovozOrder> ()
				.JoinAlias (o => o.DeliveryPoint, () => point)
				.Where (o => o.DeliveryDate == date.Date && point.LogisticsArea.Id == area.Id 
					&& !o.SelfDelivery && o.OrderStatus == Vodovoz.Domain.Orders.OrderStatus.Accepted)
				.List<Vodovoz.Domain.Orders.Order> ();
		}

		public static VodovozOrder GetLatestCompleteOrderForCounterparty(IUnitOfWork UoW, Counterparty counterparty)
		{
			VodovozOrder orderAlias = null;
			var queryResult = UoW.Session.QueryOver<Vodovoz.Domain.Orders.Order>(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterparty.Id)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.OrderBy(() => orderAlias.Id).Desc
				.Take(1).List();
			return queryResult.FirstOrDefault();
		}

		public static IList<VodovozOrder> GetCurrentOrders(IUnitOfWork UoW, Counterparty counterparty)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Where(() => orderAlias.Client.Id == counterparty.Id)
				.Where(() => orderAlias.DeliveryDate >= DateTime.Today)
				.Where(() => orderAlias.OrderStatus != OrderStatus.Closed 
					&& orderAlias.OrderStatus != OrderStatus.Canceled
					&& orderAlias.OrderStatus != OrderStatus.DeliveryCanceled
					&& orderAlias.OrderStatus != OrderStatus.NotDelivered)
				.List();
		}

		public static IList<VodovozOrder> GetCompleteOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate, PaymentType payment)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.Where(o => o.PaymentType == payment)
				.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate).List();
		}

		public static IList<VodovozOrder> GetOrdersBetweenDates(IUnitOfWork UoW, DateTime startDate, DateTime endDate)
		{
			VodovozOrder orderAlias = null;
			return UoW.Session.QueryOver<VodovozOrder>(() => orderAlias)
				.Where(() => startDate <= orderAlias.DeliveryDate && orderAlias.DeliveryDate <= endDate).List();
		}

		public static IList<VodovozOrder> GetOrdersByCode1c (IUnitOfWork uow, string[] codes1c)
		{
			return uow.Session.QueryOver<VodovozOrder> ()
				.Where(c => c.Code1c.IsIn(codes1c))
				.List<VodovozOrder> ();
		}

		/// <summary>
		/// Список последних заказов для точки.
		/// </summary>
		/// <returns>Список последних заказов для точки.</returns>
		/// <param name="UoW">IUnitOfWork</param>
		/// <param name="deliveryPoint">Точка доставки.</param>
		/// <param name="count">Требуемое количество последних заказов.</param>
		public static IList<VodovozOrder> GetLatestOrdersForCounterparty(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int count)
		{
			VodovozOrder orderAlias = null;
			var queryResult = UoW.Session.QueryOver<Vodovoz.Domain.Orders.Order>(() => orderAlias)
			    .Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.OrderBy(() => orderAlias.Id).Desc
			    .Take(count).List();
			return queryResult;
		}

		public static OrderStatus[] GetOnClosingOrderStatuses()
		{
			return new OrderStatus[] {
				OrderStatus.UnloadingOnStock,
				OrderStatus.Closed
			};
		}
	}
}

