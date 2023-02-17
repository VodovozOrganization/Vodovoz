﻿using FluentNHibernate;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping
{
	public class OrderMap : ClassMap<Domain.Orders.Order>
	{
		public OrderMap()
		{
			Table("orders");

			OptimisticLock.Version();
			Version(x => x.Version)                       .Column("version");

			Id(x => x.Id)                                 .Column("id").GeneratedBy.Native();

			Map(x => x.CreateDate)                        .Column("create_date").ReadOnly();
			Map(x => x.IsFirstOrder)                      .Column("is_first_order");
			Map(x => x.Comment)                           .Column("comment");
			Map(x => x.CommentLogist)                     .Column("comment_logist");
			Map(x => x.DeliveryDate)                      .Column("delivery_date");
			Map(x => x.SelfDelivery)                      .Column("self_delivery");
			Map(x => x.PayAfterShipment)                  .Column("pay_after_shipment");
			Map(x => x.BottlesReturn)                     .Column("bottles_return");
			Map(x => x.ExtraMoney)                        .Column("extra_money");
			Map(x => x.OrderCashSum)                      .Column("sum_to_receive");
			Map(x => x.Shipped)                           .Column("shipped");
			Map(x => x.SumDifferenceReason)               .Column("sum_difference_reason");
			Map(x => x.CollectBottles)                    .Column("collect_bottles");
			Map(x => x.Code1c)                            .Column("code1c");
			Map(x => x.Address1c)                         .Column("address_1c");
			Map(x => x.Address1cCode)                     .Column("address_1c_code");
			Map(x => x.DeliverySchedule1c)                .Column("delivery_schedule_1c");
			Map(x => x.DailyNumber)                       .Column("daily_number_1c");
			Map(x => x.ClientPhone)                       .Column("client_phone");
			Map(x => x.LastEditedTime)                    .Column("last_edited_time");
			Map(x => x.CommentManager)                    .Column("comment_manager");
			Map(x => x.ReturnedTare)                      .Column("returned_tare");
			Map(x => x.InformationOnTara)                 .Column("information_on_tara");
			Map(x => x.OnRouteEditReason)                 .Column("on_route_edit_reason");
			Map(x => x.DriverCallId)                      .Column("driver_call_id");
			Map(x => x.Trifle)                            .Column("trifle");
			Map(x => x.OnlineOrder)                       .Column("online_order");
			Map(x => x.ToClientText)                      .Column("to_client_text");
			Map(x => x.FromClientText)                    .Column("from_client_text");
			Map(x => x.IsContractCloser)                  .Column("is_contract_closer");
			Map(x => x.IsSelfDeliveryPaid)                .Column("is_self_delivery_paid");
			Map(x => x.BillDate)                          .Column("bill_date");
			Map(x => x.IsTareNonReturnReasonChangedByUser).Column("is_reason_type_changed_by_user");
			Map(x => x.HasCommentForDriver)               .Column("has_comment_for_driver");
			Map(x => x.TimeDelivered)                     .Column("time_delivered");
			Map(x => x.AddCertificates)                   .Column("add_certificates");
			Map(x => x.IsBottleStock)                     .Column("is_bottle_stock");
			Map(x => x.IsBottleStockDiscrepancy)          .Column("is_bottle_stock_discrepancy");
			Map(x => x.BottlesByStockCount)               .Column("bottles_by_stock_count");
			Map(x => x.BottlesByStockActualCount)         .Column("bottles_by_stock_actual_count");
			Map(x => x.EShopOrder)                        .Column("e_shop_order");
			Map(x => x.ContactlessDelivery)               .Column("contactless_delivery");
			Map(x => x.PaymentBySms)                      .Column("payment_by_sms");
			Map(x => x.PaymentByQr)                       .Column("payment_by_qr");
			Map(x => x.ODZComment)                        .Column("odz_comment");
			Map(x => x.OPComment)                         .Column("op_comment");
			Map(x => x.CommentOPManagerUpdatedAt)         .Column("comment_opmanager_updated_at");
			Map(x => x.IsFastDelivery)                    .Column("is_fast_delivery");
			Map(x => x.IsCopiedFromUndelivery)            .Column("is_copied_from_undelivery");
			Map(x => x.DriverMobileAppComment)            .Column("driver_app_comment");
			Map(x => x.DriverMobileAppCommentTime)        .Column("driver_app_comment_time");

			Map(x => x.OrderStatus)                       .Column("order_status")
				.CustomType<OrderStatusStringType>();
			Map(x => x.SignatureType)                     .Column("signature_type")
				.CustomType<OrderSignatureTypeStringType>();
			Map(x => x.PaymentType)                       .Column("payment_type")
				.CustomType<PaymentTypeStringType>()
				.Access.CamelCaseField(Prefix.Underscore);
			Map(x => x.DocumentType)                      .Column("document_type")
				.CustomType<DefaultDocumentTypeStringType>();
			Map(x => x.DriverCallType)                    .Column("driver_call_type")
				.CustomType<DriverCallTypeStringType>();
			Map(x => x.OrderSource)                       .Column("order_source")
				.CustomType<OrderSourceStringType>();
			Map(x => x.OrderPaymentStatus)                .Column("order_payment_status")
				.CustomType<OrderPaymentStatusStringType>();
			Map(x => x.OrderAddressType)                  .Column("order_address_type")
				.CustomType<OrderAddressTypeStringType>();

			References(x => x.Client)                     .Column("client_id");
			References(x => x.Contract)                   .Column("counterparty_contract_id").Cascade.SaveUpdate();
			References(x => x.Author)                     .Column("author_employee_id");
			References(x => x.AcceptedOrderEmployee)      .Column("accepted_order_employee");
			References(x => x.DeliveryPoint)              .Column("delivery_point_id");
			References(x => x.DeliverySchedule)           .Column("delivery_schedule_id");
			References(x => x.PreviousOrder)              .Column("previous_order_id");
			References(x => x.BottlesMovementOperation)   .Column("bottles_movement_operation_id");
			References(x => x.MoneyMovementOperation)     .Column("money_movement_operation_id");
			References(x => x.LastEditor)                 .Column("editor_employee_id");
			References(x => x.LoadAllowedBy)              .Column("load_allowed_employee_id");
			References(x => x.TareNonReturnReason)        .Column("tare_non_return_reason_id");
			References(x => x.PaymentByCardFrom)          .Column("payment_from_id").Access.CamelCaseField(Prefix.Underscore);
			References(x => x.ReturnTareReason)           .Column("return_tare_reason_id");
			References(x => x.ReturnTareReasonCategory)   .Column("return_tare_reason_category_id");
			References(x => x.OurOrganization)            .Column("our_organization_id");
			References(x => x.ContactPhone)               .Column("contact_phone_id");
			References(x => x.CommentOPManagerChangedBy)  .Column("comment_opmanager_changed_by");

			HasMany(x => x.OrderDocuments)                .KeyColumn("attached_to_order_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
			HasMany(x => x.OrderDepositItems)             .KeyColumn("order_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
			HasMany(x => x.OrderItems)                    .KeyColumn("order_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
			HasMany(x => x.OrderEquipments)               .KeyColumn("order_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
			HasMany(x => x.InitialOrderService)           .KeyColumn("initial_order_id").
				Cascade.None().Inverse().LazyLoad();
			HasMany(x => x.FinalOrderService)             .KeyColumn("final_order_id")
				.Cascade.None().Inverse().LazyLoad();
			HasMany(x => x.DepositOperations)             .KeyColumn("order_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();

			HasManyToMany(x => x.PromotionalSets)
				.Table("promotional_sets_to_orders")
				.ParentKeyColumn("order_id")
				.ChildKeyColumn("promotional_set_id")
				.LazyLoad();
		}
	}
}
