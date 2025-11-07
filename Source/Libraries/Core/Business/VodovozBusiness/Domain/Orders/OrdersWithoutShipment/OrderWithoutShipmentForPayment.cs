using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Controllers;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на постоплату",
		Nominative = "счет без отгрузки на постоплату",
		Prepositional = "счете без отгрузки на постоплату",
		PrepositionalPlural = "счетах без отгрузки на постоплату")]
	[EntityPermission]
	[HistoryTrace]
	public class OrderWithoutShipmentForPayment : OrderWithoutShipmentBase, IPrintableRDLDocument, IEmailableDocument, IValidatableObject
	{
		public virtual int Id { get; set; }
		
		IList<OrderWithoutShipmentForPaymentItem> orderWithoutDeliveryForPaymentItems = new List<OrderWithoutShipmentForPaymentItem>();
		[Display(Name = "Строки счета без отгрузки на постоплату")]
		public virtual IList<OrderWithoutShipmentForPaymentItem> OrderWithoutDeliveryForPaymentItems {
			get => orderWithoutDeliveryForPaymentItems;
			set => SetField(ref orderWithoutDeliveryForPaymentItems, value);
		}

		GenericObservableList<OrderWithoutShipmentForPaymentItem> observableOrderWithoutDeliveryForPaymentItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderWithoutShipmentForPaymentItem> ObservableOrderWithoutDeliveryForPaymentItems {
			get {
				if(observableOrderWithoutDeliveryForPaymentItems == null) {
					observableOrderWithoutDeliveryForPaymentItems = new GenericObservableList<OrderWithoutShipmentForPaymentItem>(orderWithoutDeliveryForPaymentItems);
				}

				return observableOrderWithoutDeliveryForPaymentItems;
			}
		}

		public virtual void AddOrder(Order orderToAdd)
		{
			if(ObservableOrderWithoutDeliveryForPaymentItems
				.SingleOrDefault(x => x.Order.Id == orderToAdd.Id) != null)
			{
				return;
			}
			
			var item = new OrderWithoutShipmentForPaymentItem
			{
				Order = orderToAdd,
				OrderWithoutDeliveryForPayment = this
			};
			
			AddItem(item);
		}
		
		protected virtual void AddItem(OrderWithoutShipmentForPaymentItem item)
		{
			ObservableOrderWithoutDeliveryForPaymentItems.Add(item);
		}
		
		public virtual void RemoveItem(Order orderToRemove)
		{
			var item = ObservableOrderWithoutDeliveryForPaymentItems
				.SingleOrDefault(x => x.Order.Id == orderToRemove.Id);
			
			if(item != null)
			{
				ObservableOrderWithoutDeliveryForPaymentItems.Remove(item);
			}
		}
		
		public virtual OrderDocumentType Type => OrderDocumentType.BillWSForPayment;

		private Order order;
		public virtual Order Order
		{
			get => order;
			set
			{
				if (value != null)
				{
					SetField(ref order, value);
				}
			}
		}

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var settings = ScopeProvider.Scope.Resolve<IOrganizationSettings>();
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Identifier = "Documents.BillWithoutShipmentForPayment";
			reportInfo.Title = Title;
			reportInfo.Parameters = new Dictionary<string, object> {
				{ "bill_ws_for_payment_id", Id },
				{ "special_contract_number", SpecialContractNumber },
				{ "organization_id", Organization.Id },
				{ "hide_signature", HideSignature },
				{ "special", false }
			};
			return reportInfo;
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public virtual string Title => string.Format($"Счет №Ф{Id} от {CreateDate:d} {SpecialContractNumber}");

		public virtual string Name => string.Format($"Счет №Ф{Id}");
		
		public virtual string SpecialContractNumber => Client.IsForRetail ? Client.GetSpecialContractString() : string.Empty;

		public virtual DateTime? DocumentDate => CreateDate;
		public virtual Counterparty Counterparty => Client;

		public virtual PrinterType PrintType => PrinterType.RDL;
		public virtual DocumentOrientation Orientation => DocumentOrientation.Portrait;

		int copiesToPrint = 1;
		public virtual int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}
		
		#region Свои свойства

		private bool hideSignature;
		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature {
			get => hideSignature;
			set => SetField(ref hideSignature, value);
		}

		#endregion

		public virtual EmailTemplate GetEmailTemplate(ICounterpartyEdoAccountController edoAccountController = null)
		{
			var template = new EmailTemplate();

			template.Title = "ООО \"Веселый водовоз\"";
			template.Text =
						"Здравствуйте,\n" +
						"Для Вас, сформирован счет для оплаты.\n" +
						"Мы стремимся стать лучше и хотим еще больше радовать наших " +
						"клиентов! Поэтому нам так важно услышать ваше мнение. " +
						"Расскажите, что нам следовало бы изменить в нашей работе," +
						"и мы обязательно учтем ваши рекомендации.\n" +
						"Так же рады предложить Вам,\n" +
						"нашу новую услугу - Санитарная обработка кулера с озонацией." +
						"Ведь качественная питьевая вода - это хорошо," +
						"но еще лучше оборудование, очищенное от микробов." +
						"Вы можете оформить заказ в любое удобное время. Мы работаем 24 часа и 7 дней в неделю.\n" +
						"Спасибо, что Вы с нами.\n\n" +
						"С Уважением,\n" +
						"Команда компании  \"Веселый Водовоз\"\n" +
						"тел.: +7(812) 317-00-00\n" +
						"P.S.И помни, мы тебя любим!\n\n" +
						"Мы ВКонтакте: vk.com/vodovoz_spb\n" +
						"Мы в Instagram: @vodovoz_lifestyle\n" +
						"Наш официальный сайт: www.vodovoz-spb.ru";
			template.TextHtml =
						"<p>Здравствуйте</p>\n" +
						"<p>Для Вас сформирован счет для оплаты.</p>\n" +
						"<p>Мы стремимся стать лучше и хотим еще больше радовать наших клиентов! Поэтому нам так важно услышать ваше мнение. Расскажите, что нам следовало бы изменить в нашей работе, и мы обязательно учтем ваши рекомендации.</p>\n" +
						"<p>Так же рады предложить Вам, нашу новую услугу - Санитарная обработка кулера с озонацией. Ведь качественная питьевая вода - это хорошо, но еще лучше оборудование, очищенное от микробов.</p>\n" +
						"<p>Вы можете оформить заказ в любое удобное время. Мы работаем 24 часа и 7 дней в неделю.</p>\n" +
						"<p>Спасибо, что Вы с нами.</p>\n" +
						"<p>С Уважением,</p>\n" +
						"<p>Команда компании  \"Веселый Водовоз\"</p>\n" +
						"<p>тел.: +7 (812) 317-00-00</p>\n" +
						"<p>P.S. И помни, мы тебя любим!</p>\n" +
						"<p>______________</p>\n" +
						"<p>Мы ВКонтакте: <a href=\"https://vk.com/vodovoz_spb\" target=\"_blank\">vk.com/vodovoz_spb</a></p>\n" +
						"<p>Мы в Instagram: @vodovoz_lifestyle</p>\n" +
						"<p>Наш официальный сайт: <a href=\"http://www.vodovoz-spb.ru/\" target=\"_blank\">www.vodovoz-spb.ru</a></p>\n" +
						"<img src=\"https://cloud1.vod.qsolution.ru/email-attachments/email_ad.png\">";

			return template;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Client == null)
				yield return new ValidationResult(
					"Необходимо заполнить контрагента.",
					new[] {nameof(Client)}
				);
			
			if(!OrderWithoutDeliveryForPaymentItems.Any())
				yield return new ValidationResult(
					"Необходимо добавить заказы в счет.",
					new[] {nameof(OrderWithoutDeliveryForPaymentItems)}
				);
		}
	} 
}
