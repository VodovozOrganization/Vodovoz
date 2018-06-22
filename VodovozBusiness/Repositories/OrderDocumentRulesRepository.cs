using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using System;
using System.Linq;

namespace Vodovoz.Repositories
{
	class Rule
	{
		public Func<KeyToDocumentsSet, bool> Condition;
		public OrderDocumentType[] Documents;

		public Rule(Func<KeyToDocumentsSet, bool> condition, OrderDocumentType[] documents)
		{
			Condition = condition;
			Documents = documents;
		}
	}

	public static class OrderDocumentRulesRepository
	{
		static List<Rule> rules = new List<Rule>();

		public static OrderDocumentType[] GetSetOfDocumets(KeyToDocumentsSet keys) =>
		rules.Where(r => r.Condition(keys)).SelectMany(r => r.Documents).Distinct().ToArray();

		static OrderDocumentRulesRepository()
		{
			//Invoice
			rules.Add(
				new Rule(
					keys => GetConditionForInvoice(keys),
					new[] {
						OrderDocumentType.Invoice
					}
				)
			);
			//InvoiceBarter
			rules.Add(
				new Rule(
					keys => GetConditionForBarterInvoice(keys),
					new[] {
						OrderDocumentType.InvoiceBarter
					}
				)
			);
			//EquipmentTransfer
			rules.Add(
				new Rule(
					keys => GetConditionForEquipmentTransfer(keys),
					new[] {
						OrderDocumentType.EquipmentTransfer
					}
				)
			);
			//DriverTicket
			rules.Add(
				new Rule(
					keys => GetConditionForDriverTicket(keys),
					new[] {
						OrderDocumentType.DriverTicket
					}
				)
			);
			//UPD
			rules.Add(
				new Rule(
					keys => GetConditionForUPD(keys),
					new[] {
						OrderDocumentType.UPD
					}
				)
			);
			//Bill
			rules.Add(
				new Rule(
					keys => GetConditionForBill(keys),
					new[] {
						OrderDocumentType.Bill
					}
				)
			);
			//TORG12+SF
			rules.Add(
				new Rule(
					keys => GetConditionForTORG12(keys),
					new[] {
						OrderDocumentType.Torg12,
						OrderDocumentType.ShetFactura
					}
				)
			);
		}

		static bool GetConditionForInvoice(KeyToDocumentsSet keys) =>
		(
			(
				(
					(
						keys.PaymentType == PaymentType.cashless
						&& keys.IsPriceOfAllOrderItemsZero
					)
					&&
					(
						!keys.NeedToRefundDepositToClient
						|| keys.NeedToReturnBottles
					)
				)
				||
				(
					keys.PaymentType == PaymentType.ByCard
					&& keys.HasOrderItems
				)
				|| keys.PaymentType == PaymentType.cash
			)
			&& keys.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForBarterInvoice(KeyToDocumentsSet keys) =>
		(
			keys.PaymentType == PaymentType.barter
			&& keys.OrderStatus >= OrderStatus.Accepted
		);


		static bool GetConditionForEquipmentTransfer(KeyToDocumentsSet keys) =>
		(
			(
				(
					keys.HasOrderEquipment
					&& keys.PaymentType == PaymentType.cash
					|| keys.PaymentType == PaymentType.barter
				)
				||
				(
					keys.HasOrderEquipment
					&& keys.PaymentType == PaymentType.ByCard
					&& !keys.IsPriceOfAllOrderItemsZero
				)
				||
				(
					keys.HasOrderEquipment
					&& keys.PaymentType == PaymentType.cashless
					&& !keys.NeedToRefundDepositToClient
				)
				|| keys.NeedMaster
			)
			&& keys.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForDriverTicket(KeyToDocumentsSet keys) =>
		(
			GetConditionForBill(keys)
			&& !keys.IsSelfDelivery
			&& keys.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForUPD(KeyToDocumentsSet keys) =>
		(
			GetConditionForBill(keys)
			&& keys.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForBill(KeyToDocumentsSet keys) =>
		(
			keys.PaymentType == PaymentType.cashless
			&& !keys.IsPriceOfAllOrderItemsZero
			&& !keys.NeedToRefundDepositToClient
			&& keys.HasOrderItems
		);

		static bool GetConditionForTORG12(KeyToDocumentsSet keys) =>
		(
			GetConditionForUPD(keys)
			&& keys.DefaultDocumentType == DefaultDocumentType.torg12
		);

		public static Dictionary<KeyToDocumentsSet, OrderDocumentType[]> GetAllRulesForDocumetsCollecting()
		{
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
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] { }
			);

			//rule#4.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					HasOrderItems = true,
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#4.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					HasOrderItems = true,
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
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
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#6.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					HasOrderEquipment = true,
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#6.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					HasOrderEquipment = true,
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] { }
			);

			//rule#7.1
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cash,
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.Invoice
				}
			);
			//rule#7.1.1 Barter
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.barter,
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] {
					OrderDocumentType.InvoiceBarter
				}
			);
			//rule#7.2
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.ByCard,
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#7.3
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					NeedToRefundDepositToClient = true
				},
				new OrderDocumentType[] { }
			);
			//rule#7.4
			rulesForOrderDocumentsColecting.Add(
				new KeyToDocumentsSet {
					PaymentType = PaymentType.cashless,
					IsPriceOfAllOrderItemsZero = true,
					NeedToRefundDepositToClient = true
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
	}
}
