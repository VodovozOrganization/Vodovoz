using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Settings.Organizations;

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
		private static IOrganizationSettings _organizationSettings => ScopeProvider.Scope.Resolve<IOrganizationSettings>();
		
		private static int _beveragesWorldOrganizationId => _organizationSettings.BeveragesWorldOrganizationId;

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
					GetConditionForSpecialUPD,
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
			//TORG12
			rules.Add(
				new Rule(
					key => GetConditionForTorg12(key),
					new[] {
						OrderDocumentType.Torg12
					}
				)
			);
			//ShetFactura
			rules.Add(
				new Rule(
					key => GetConditionForShetFactura(key),
					new[] {
						OrderDocumentType.ShetFactura
					}
				)
			);
			//TransportInvoice
			rules.Add(
				new Rule(
					key => GetConditionForTransportInvoice(key),
					new[] {
						OrderDocumentType.TransportInvoice
					}
				)
			);

			//Torg2
			rules.Add(
				new Rule(
					key => GetConditionForTorg2(key),
					new[] {
						OrderDocumentType.Torg2
					}
				)
			);

			//AssemblyListDocument
			rules.Add(
				new Rule(
					key => GetConditionForAssemblyList(key),
					new[] {
						OrderDocumentType.AssemblyList
					}
				)
			);
		}

		static bool GetConditionForAssemblyList(OrderStateKey key)
		{
			return key.HasEShopOrder;
		}

		static bool GetConditionForInvoice(OrderStateKey key)
		{
			var acceptedOrAfter = key.OrderStatus >= OrderStatus.Accepted;
			var waitForPaymentOrAfter = key.OrderStatus >= OrderStatus.WaitForPayment;

			var notNeedToRefundDepositOrNeedToReturnBottles = !key.NeedToRefundDepositToClient || key.NeedToReturnBottles;

			var cashless = key.PaymentType == PaymentType.Cashless;
			var paidOnline = (key.PaymentType == PaymentType.PaidOnline || key.PaymentType == PaymentType.Terminal) && key.HasOrderItems;
			var cash = key.PaymentType == PaymentType.Cash;
			var fastPaymentQr = (key.PaymentType == PaymentType.DriverApplicationQR || key.PaymentType == PaymentType.SmsQR) && key.HasOrderItems;

			if(key.IsSelfDelivery)
			{
				return (cashless || paidOnline || cash || fastPaymentQr) && waitForPaymentOrAfter;
			}
			else
			{
				return ((cashless && key.IsPriceOfAllOrderItemsZero && notNeedToRefundDepositOrNeedToReturnBottles) || paidOnline || cash || fastPaymentQr) && acceptedOrAfter;
			}
		}

		static bool GetConditionForTorg2(OrderStateKey key)
		{
			return key.Order.Client.Torg2Count.HasValue;
		}

		static bool GetConditionForTransportInvoice(OrderStateKey key)
		{
			return key.Order.Client.TTNCount.HasValue && key.HasOrderItems;
		}

		static bool GetConditionForBarterInvoice(OrderStateKey key) =>
		(
			key.PaymentType == PaymentType.Barter
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool GetConditionForContractDocumentationInvoice(OrderStateKey key) =>
		(
			key.PaymentType == PaymentType.ContractDocumentation
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
			key.PaymentType == PaymentType.Cashless
			&& IsOrderWithOrderItemsAndWithoutDeposits(key)
			&& key.OrderStatus >= OrderStatus.Accepted
		);

		static bool ConditionForUPD(OrderStateKey key)
		{
			var billCondition = key.HaveSpecialFields
				? GetConditionForSpecialBill(key) 
				: GetConditionForBill(key);

			return (
				(billCondition ||
				 (key.Order.Client.UPDCount.HasValue
				  && ((key.Order.OurOrganization != null && key.Order.OurOrganization.Id == _beveragesWorldOrganizationId)
				      || (key.Order.Client?.WorksThroughOrganization != null
				          && key.Order.Client.WorksThroughOrganization.Id == _beveragesWorldOrganizationId))
				  && IsOrderWithOrderItemsAndWithoutDeposits(key)))
				&& (key.OrderStatus >= OrderStatus.Accepted ||
					(key.OrderStatus == OrderStatus.WaitForPayment && key.IsSelfDelivery && key.PayAfterShipment))
			);
		}

		static bool GetConditionForUPD(OrderStateKey key) =>
		(	
			ConditionForUPD(key)
			&& !key.HaveSpecialFields
			&& !key.Order.OrderItems.All(x => x.Nomenclature.Category == NomenclatureCategory.deposit)
		);

		static bool GetConditionForSpecialUPD(OrderStateKey key) =>
		(
			ConditionForUPD(key)
			&& key.HaveSpecialFields
			&& !key.Order.OrderItems.All(x => x.Nomenclature.Category == NomenclatureCategory.deposit)
		);

		static bool GetConditionForBill(OrderStateKey key) =>
		(
			key.PaymentType == PaymentType.Cashless
			&& IsOrderWithOrderItemsAndWithoutDeposits(key)
			&& !key.HaveSpecialFields
		);

		static bool GetConditionForSpecialBill(OrderStateKey key, OrderDocumentType[] documentTypes = null) =>
		(
			key.PaymentType == PaymentType.Cashless
			&& IsOrderWithOrderItemsAndWithoutDeposits(key)
			&& key.HaveSpecialFields
		);

		static bool GetConditionForTorg12(OrderStateKey key) =>
		(
			(key.Order.IsCashlessPaymentTypeAndOrganizationWithoutVAT
			|| ConditionForUPD(key))
			&& key.DefaultDocumentType == DefaultDocumentType.torg12
		);
		
		static bool GetConditionForShetFactura(OrderStateKey key) =>
		(
			!key.Order.IsCashlessPaymentTypeAndOrganizationWithoutVAT
			&& ConditionForUPD(key)
			&& key.DefaultDocumentType == DefaultDocumentType.torg12
		);

		static bool IsOrderWithOrderItemsAndWithoutDeposits(OrderStateKey key) =>
		(
			!key.IsPriceOfAllOrderItemsZero
			&& !key.NeedToRefundDepositToClient
			&& key.HasOrderItems
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
			rules.Add(new ProhibitionRule(GetConditionForDepositReturn, "Возврат залога не применим для заказа в текущем состоянии"));
			rules.Add(new ProhibitionRule(GetConditionForOrderWithoutMoney, "Невозможно подтвердить или перевести в статус ожидания оплаты заказ в текущем состоянии без суммы"));
			rules.Add(new ProhibitionRule(GetConditionForEmptyOrder, "Невозможно подтвердить или перевести в статус ожидания оплаты пустой заказ"));
		}

		/// <summary>
		/// Причина запрета: Возврат залога не применим для заказа в текущем состоянии
		/// </summary>
		static bool GetConditionForDepositReturn(OrderStateKey key)
		{
			bool result = false;
			if(!key.NeedToRefundDepositToClient) {
				return result;
			}

			if(key.PaymentType == PaymentType.Cashless && key.HasOrderItems) {
				result = true;
			}

			if((key.PaymentType == PaymentType.Cashless || 
			    key.PaymentType == PaymentType.PaidOnline || 
			    key.PaymentType == PaymentType.Terminal) && !key.HasOrderItems) {
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

			if(key.HasOrderItems || key.NeedToRefundDepositToClient) {
				return false;
			}

			if((key.PaymentType == PaymentType.PaidOnline || key.PaymentType == PaymentType.Terminal) &&
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
