using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Parameters;
using NPOI.SS.Formula.Functions;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на предоплату",
		Nominative = "счет без отгрузки на предоплату",
		Prepositional = "счете без отгрузки на предоплату",
		PrepositionalPlural = "счетах без отгрузки на предоплату")]
	[EntityPermission]
	[HistoryTrace]
	public class OrderWithoutShipmentForAdvancePayment : OrderWithoutShipmentBase, IPrintableRDLDocument, IEmailableDocument, IValidatableObject
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

		public virtual void RecalculateItemsPrice()
		{
			foreach(OrderWithoutShipmentForAdvancePaymentItem item in ObservableOrderWithoutDeliveryForAdvancePaymentItems) {
				if(item.Nomenclature.Category == NomenclatureCategory.water) {
					item.RecalculatePrice();
				}
			}
		}

		public virtual int GetTotalWater19LCount()
		{
			var water19L =
				ObservableOrderWithoutDeliveryForAdvancePaymentItems.Where(x => x.Nomenclature.IsWater19L);

			return (int)water19L.Sum(x => x.Count);
		}

		public virtual void AddNomenclature(Nomenclature nomenclature, int count = 0, decimal discount = 0, bool discountInMoney = false, DiscountReason discountReason = null, PromotionalSet proSet = null)
		{
			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
			                               && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			OrderWithoutShipmentForAdvancePaymentItem oi = new OrderWithoutShipmentForAdvancePaymentItem {
				OrderWithoutDeliveryForAdvancePayment = this,
				Count = count,
				Nomenclature = nomenclature,
				Price = nomenclature.GetPrice(1, canApplyAlternativePrice),
				IsDiscountInMoney = discountInMoney,
				DiscountSetter = discount,
				DiscountReason = discountReason
			};
			AddItemWithNomenclatureForSale(oi);
		}

		public virtual void AddItemWithNomenclatureForSale(OrderWithoutShipmentForAdvancePaymentItem orderItem)
		{
			var canApplyAlternativePrice = HasPermissionsForAlternativePrice 
			                               && orderItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= orderItem.Count);
			var acceptableCategories = Nomenclature.GetCategoriesForSale();
			if(orderItem?.Nomenclature == null || !acceptableCategories.Contains(orderItem.Nomenclature.Category))
				return;

			orderItem.IsAlternativePrice = canApplyAlternativePrice;
			ObservableOrderWithoutDeliveryForAdvancePaymentItems.Add(orderItem);
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
			return new ReportInfo {
				Title = this.Title,
				Identifier = "Documents.BillWithoutShipmentForAdvancePayment",
				Parameters = new Dictionary<string, object> {
					{ "bill_ws_for_advance_payment_id", Id },
					{ "special_contract_number", SpecialContractNumber },
					{ "organization_id", new OrganizationParametersProvider(new ParametersProvider()).GetCashlessOrganisationId },
					{ "hide_signature", HideSignature },
					{ "special", false }
				}
			};
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

		public virtual EmailTemplate GetEmailTemplate()
		{
			var template = new EmailTemplate();

			var imageId = "email_ad";
			var image = new EmailAttachment();
			image.MIMEType = "image/png";
			image.FileName = "email_ad.png";
			using(Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Vodovoz.Resources.email_ad.png")) {
				byte[] buffer = new byte[stream.Length];
				stream.Read(buffer, 0, buffer.Length);
				image.Base64Content = Convert.ToBase64String(buffer);
			}

			template.Attachments.Add(imageId, image);

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
						string.Format("<img src=\"cid:{0}\">", imageId);

			return template;
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Client == null)
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
	}
}
