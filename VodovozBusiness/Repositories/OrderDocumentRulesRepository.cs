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

		public static OrderDocumentType[] GetSetOfDocumets(KeyToDocumentsSet key) =>
		rules.Where(r => r.Condition(key)).SelectMany(r => r.Documents).Distinct().ToArray();

		static OrderDocumentRulesRepository()
		{
			//Invoice
			rules.Add(
				new Rule(
					key => GetConditionForInvoice(key),
					new[] {
						OrderDocumentType.Invoice
					}
				)
			);
			//InvoiceBarter
			rules.Add(
				new Rule(
					key => GetConditionForBarterInvoice(key),
					new[] {
						OrderDocumentType.InvoiceBarter
					}
				)
			);
			//EquipmentTransfer
			rules.Add(
				new Rule(
					key => GetConditionForEquipmentTransfer(key),
					new[] {
						OrderDocumentType.EquipmentTransfer
					}
				)
			);
			//DriverTicket
			rules.Add(
				new Rule(
					key => GetConditionForDriverTicket(key),
					new[] {
						OrderDocumentType.DriverTicket
					}
				)
			);
			//UPD
			rules.Add(
				new Rule(
					key => GetConditionForUPD(key),
					new[] {
						OrderDocumentType.UPD
					}
				)
			);
			//Bill
			rules.Add(
				new Rule(
					key => GetConditionForBill(key),
					new[] {
						OrderDocumentType.Bill
					}
				)
			);
			//TORG12+SF
			rules.Add(
				new Rule(
					key => GetConditionForTORG12(key),
					new[] {
						OrderDocumentType.Torg12,
						OrderDocumentType.ShetFactura
					}
				)
			);
		}

		static bool GetConditionForInvoice(KeyToDocumentsSet key) =>
		(
			(
				(
					(
						key.PaymentType == PaymentType.cashless
						&& key.IsPriceOfAllOrderItemsZero
					)
					&&
					(
						!key.NeedToRefundDepositToClient
						|| key.NeedToReturnBottles
					)
				)
				||
				(
					key.PaymentType == PaymentType.ByCard
					&& key.HasOrderItems
				)
				|| key.PaymentType == PaymentType.cash
			)
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForBarterInvoice(KeyToDocumentsSet key) =>
		(
			key.PaymentType == PaymentType.barter
			&& key.OrderStatus >= OrderStatus.Accepted
		);


		static bool GetConditionForEquipmentTransfer(KeyToDocumentsSet key) =>
		(
			(
				(
					key.HasOrderEquipment
					&&
					(
						key.PaymentType == PaymentType.cash
					    || key.PaymentType == PaymentType.barter
					)
				)
				||
				(
					key.HasOrderEquipment
					&& key.PaymentType == PaymentType.ByCard
					&& !key.IsPriceOfAllOrderItemsZero
				)
				||
				(
					key.HasOrderEquipment
					&& key.PaymentType == PaymentType.cashless
					&& !key.NeedToRefundDepositToClient
				)
				|| key.NeedMaster
			)
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForDriverTicket(KeyToDocumentsSet key) =>
		(
			GetConditionForBill(key)
			&& !key.IsSelfDelivery
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForUPD(KeyToDocumentsSet key) =>
		(
			GetConditionForBill(key)
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForBill(KeyToDocumentsSet key) =>
		(
			key.PaymentType == PaymentType.cashless
			&& !key.IsPriceOfAllOrderItemsZero
			&& !key.NeedToRefundDepositToClient
			&& key.HasOrderItems
		);

		static bool GetConditionForTORG12(KeyToDocumentsSet key) =>
		(
			GetConditionForUPD(key)
			&& key.DefaultDocumentType == DefaultDocumentType.torg12
		);
	}
}