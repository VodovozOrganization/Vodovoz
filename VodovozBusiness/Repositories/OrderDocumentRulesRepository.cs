using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Repositories
{
	public static class OrderDocumentRulesRepository
	{
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
	}
}
