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
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.StoredEmails;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на предоплату",
		Nominative = "счет без отгрузки на предоплату",
		Prepositional = "счете без отгрузки на предоплату",
		PrepositionalPlural = "счетах без отгрузки на предоплату")]
	[EntityPermission]
	[HistoryTrace]
	public class OrderWithoutShipmentForAdvancePayment :
		OrderWithoutShipmentBase,
		IDomainObject,
		IPrintableRDLDocument,
		IEmailableDocument,
		IRecalculateTaxSource,
		ISaleSource,
		IValidatableObject
	{
		public virtual int Id { get; set; }
		
		IList<OrderWithoutShipmentForAdvancePaymentItem> orderWithoutDeliveryForAdvancePaymentItems = new List<OrderWithoutShipmentForAdvancePaymentItem>();
		[Display(Name = "Строки счета без отгрузки на предоплату")]
		public virtual IList<OrderWithoutShipmentForAdvancePaymentItem> OrderWithoutDeliveryForAdvancePaymentItems {
			get => orderWithoutDeliveryForAdvancePaymentItems;
			set => SetField(ref orderWithoutDeliveryForAdvancePaymentItems, value);
		}

		GenericObservableList<OrderWithoutShipmentForAdvancePaymentItem> observableOrderWithoutDeliveryForAdvancePaymentItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderWithoutShipmentForAdvancePaymentItem> ObservableOrderWithoutDeliveryForAdvancePaymentItems {
			get {
				if(observableOrderWithoutDeliveryForAdvancePaymentItems == null) {
					observableOrderWithoutDeliveryForAdvancePaymentItems =
						new GenericObservableList<OrderWithoutShipmentForAdvancePaymentItem>(OrderWithoutDeliveryForAdvancePaymentItems);
				}

				return observableOrderWithoutDeliveryForAdvancePaymentItems;
			}
		}

		public virtual void AddNewNomenclatureWithoutDiscount(
			Nomenclature nomenclature,
			ISaleHandler saleHandler,
			int count = 0,
			PromotionalSet proSet = null)
		{
			var canApplyAlternativePrice =
				HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			var oi = OrderWithoutShipmentForAdvancePaymentItem
				.Create(this, count, nomenclature, nomenclature.GetPrice(1, canApplyAlternativePrice));

			AddItemWithNomenclatureForSale(oi, saleHandler);
		}

		private void AddItemWithNomenclatureForSale(
			OrderWithoutShipmentForAdvancePaymentItem saleItem,
			ISaleHandler saleHandler)
		{
			var canApplyAlternativePrice =
				HasPermissionsForAlternativePrice 
				&& saleItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= saleItem.Count);

			var acceptableCategories = NomenclatureEntity.GetCategoriesForSale();
			
			if(saleItem?.Nomenclature == null || !acceptableCategories.Contains(saleItem.Nomenclature.Category))
			{
				return;
			}

			saleItem.IsAlternativePrice = canApplyAlternativePrice;
			ObservableOrderWithoutDeliveryForAdvancePaymentItems.Add(saleItem);
			saleHandler.Recalculate();
		}

		public virtual void RemoveItem(OrderWithoutShipmentForAdvancePaymentItem item)
		{
			ObservableOrderWithoutDeliveryForAdvancePaymentItems.Remove(item);
		}
		
		public virtual OrderDocumentType Type => OrderDocumentType.BillWSForAdvancePayment;

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
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Identifier = "Documents.BillWithoutShipmentForAdvancePayment";
			reportInfo.Title = Title;
			reportInfo.Parameters = new Dictionary<string, object> {
				{ "bill_ws_for_advance_payment_id", Id },
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

		CounterpartyEntity IEmailableDocument.Counterparty => Client;

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

		public virtual EmailTemplate GetEmailTemplate(
			ICounterpartyEdoAccountEntityController edoAccountController = null,
			IOrganizationSettings organizationSettings = null,
			IDeliveryScheduleSettings deliveryScheduleSettings = null)
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
			if(Organization == null)
			{
				yield return new ValidationResult(
					"Необходимо заполнить организацию.",
					new[] { nameof(Organization) }
				);
			}

			if(Client == null)
				yield return new ValidationResult(
					"Необходимо заполнить контрагента.",
					new[] {nameof(Client)}
				);
			
			if(!OrderWithoutDeliveryForAdvancePaymentItems.Any())
				yield return new ValidationResult(
					"Необходимо добавить товары в счет.",
					new[] {nameof(OrderWithoutDeliveryForAdvancePaymentItems)}
				);
		}

		public virtual int DocumentId => Id;

		#region IRecalculateTaxSource implementation

		public virtual DateTime? DeliveryDate => null;
		IUsnModeOrganization IRecalculateTaxSource.Organization => Organization;

		#endregion

		#region ISaleSource implementation

		public virtual DeliveryPoint DeliveryPoint => null;
		public virtual Counterparty Counterparty => Client;

		public virtual IEnumerable<ISaleItem> SaleItems => OrderWithoutDeliveryForAdvancePaymentItems;

		#endregion
	}
}
