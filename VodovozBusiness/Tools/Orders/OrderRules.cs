using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using System;
using System.Linq;

namespace Vodovoz.Tools.Orders
{
	class Rule
	{
		public Func<OrderStateKey, bool> Condition;
		public OrderDocumentType[] Documents;

		public Rule(Func<OrderStateKey, bool> condition, OrderDocumentType[] documents)
		{
			Condition = condition;
			Documents = documents;
		}
	}

	public static class OrderDocumentRulesRepository
	{
		static List<Rule> rules = new List<Rule>();

		public static OrderDocumentType[] GetSetOfDocumets(OrderStateKey key) =>
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
			//ContractDocumentationInvoice
			rules.Add(
				new Rule(
					key => GetConditionForContractDocumentationInvoice(key),
					new[] {
						OrderDocumentType.InvoiceContractDoc
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
			//EquipmentReturn
			rules.Add(
				new Rule(
					key => GetConditionForEquipmentReturn(key),
					new[] {
						OrderDocumentType.EquipmentReturn
					}
				)
			);
			//DoneWorkReport
			rules.Add(
				new Rule(
					key => GetConditionForEquipmentDoneWork(key),
					new[] {
						OrderDocumentType.DoneWorkReport
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
			//Special UPD
			rules.Add(
				new Rule(
					key => GetConditionForSpecialUPD(key),
					new[] {
						OrderDocumentType.SpecialUPD
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
			//Special Bill
			rules.Add(
				new Rule(
					key => GetConditionForSpecialBill(key),
					new[] {
						OrderDocumentType.SpecialBill
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

		static bool GetConditionForInvoice(OrderStateKey key) =>
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
				|| (key.PaymentType == PaymentType.cash || key.PaymentType == PaymentType.BeveragesWorld)
			)
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForBarterInvoice(OrderStateKey key) =>
		(
			key.PaymentType == PaymentType.barter
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForContractDocumentationInvoice(OrderStateKey key) =>
		(
			key.PaymentType == PaymentType.ContractDoc
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForEquipmentTransfer(OrderStateKey key) =>
		(
			key.OrderStatus >= OrderStatus.Accepted
			&& key.OnlyEquipments.Any(e => (e.Direction == Direction.PickUp && e.DirectionReason != DirectionReason.Rent)
			                                 || (e.Direction == Direction.Deliver && (e.OwnType == OwnTypes.Duty || e.DirectionReason == DirectionReason.Rent))
			                                )
		);

		static bool GetConditionForEquipmentReturn(OrderStateKey key) =>
		(
			key.OrderStatus >= OrderStatus.Accepted
			&& key.OnlyEquipments.Any(e => e.Direction == Direction.PickUp && e.DirectionReason == DirectionReason.Rent && e.OwnType == OwnTypes.Rent)
		);

		static bool GetConditionForEquipmentDoneWork(OrderStateKey key) =>
		(
			key.OrderStatus >= OrderStatus.Accepted
			&& (
				//Условие для оборудования возвращенного из ремонта.
				key.OnlyEquipments.Any(e => e.Direction == Direction.Deliver && (e.DirectionReason == DirectionReason.Repair || e.DirectionReason == DirectionReason.RepairAndCleaning || e.DirectionReason == DirectionReason.Cleaning))
			   ||
				//Условия для выезда мастера.
				key.NeedMaster
			   )
		);

		static bool GetConditionForDriverTicket(OrderStateKey key) =>
		(
			GetConditionForBill(key)
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForUPD(OrderStateKey key) =>
		(
			GetConditionForBill(key)
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForSpecialUPD(OrderStateKey key) =>
		(
			GetConditionForUPD(key)
			&& key.HaveSpecialFields
		);

		static bool GetConditionForBill(OrderStateKey key) =>
		(
			key.PaymentType == PaymentType.cashless
			&& !key.IsPriceOfAllOrderItemsZero
			&& !key.NeedToRefundDepositToClient
			&& key.HasOrderItems
		);

		static bool GetConditionForSpecialBill(OrderStateKey key) =>
	(
		GetConditionForBill(key)
		&& key.HaveSpecialFields
	);

		static bool GetConditionForTORG12(OrderStateKey key) =>
		(
			GetConditionForUPD(key)
			&& key.DefaultDocumentType == DefaultDocumentType.torg12
		);
	}


	class ProhibitionRule
	{
		public Func<OrderStateKey, bool> Condition;
		public string Message;

		public ProhibitionRule(Func<OrderStateKey, bool> condition, string message)
		{
			Condition = condition;
			Message = message;
		}
	}

	/// <summary>
	/// Содержит правила запрета подтверждения заказа
	/// </summary>
	public static class OrderAcceptProhibitionRulesRepository
	{
		static List<ProhibitionRule> rules = new List<ProhibitionRule>();

		public static bool CanAcceptOrder(OrderStateKey key, ref List<string> messages)
		{
			messages = rules.Where(x => x.Condition(key)).Select(x => x.Message).ToList();

			return !messages.Any();
		}

		static OrderAcceptProhibitionRulesRepository()
		{
			rules.Add(new ProhibitionRule(key => GetConditionForDepositReturn(key), 
			                              "Возврат залога не применим для заказа в текущем состоянии"));
			rules.Add(new ProhibitionRule(key => GetConditionForOrderWithoutMoney(key), 
			                              "Невозможно подтвердить или перевести в статус ожидания оплаты заказ в текущем состоянии без суммы"));
			rules.Add(new ProhibitionRule(key => GetConditionForEmptyOrder(key), 
			                              "Невозможно подтвердить или перевести в статус ожидания оплаты пустой заказ"));
		}

		/// <summary>
		/// Причина запрета: Возврат залога не применим для заказа в текущем состоянии
		/// </summary>
		static bool GetConditionForDepositReturn(OrderStateKey key){
			bool result = false;
			if(!key.NeedToRefundDepositToClient) {
				return result;
			}

			if(key.PaymentType == PaymentType.cashless && key.HasOrderItems) {
				result = true;
			}

			if((key.PaymentType == PaymentType.cashless || key.PaymentType == PaymentType.ByCard) 
			   && !key.HasOrderItems) {
				result = true;
			}
			return result;
		}

		/// <summary>
		/// Причина запрета: Невозможно подтвердить заказ в текущем состоянии без суммы
		/// </summary>
		static bool GetConditionForOrderWithoutMoney(OrderStateKey key)
		{
			bool result = false;

			if(key.HasOrderItems || key.NeedToRefundDepositToClient){
				return false;
			}

			if(key.PaymentType == PaymentType.ByCard && 
			   (key.HasOrderEquipment || (!key.HasOrderEquipment && key.NeedToReturnBottles))) {
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Причина запрета: Невозможно подтвердить пустой заказ
		/// </summary>
		static bool GetConditionForEmptyOrder(OrderStateKey key)
		{
			if(!key.HasOrderItems 
			   && !key.NeedToRefundDepositToClient 
			   && !key.NeedToReturnBottles
			   && !key.HasOrderEquipment
			   && !key.HasOrderItems) {
				return true;
			}

			return false;
		}
	}
}