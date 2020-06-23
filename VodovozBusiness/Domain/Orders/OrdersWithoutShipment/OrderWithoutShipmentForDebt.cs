using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Print;
using QS.Report;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы без отгрузки на долги",
		Nominative = "заказ без отгрузки на долг",
		Prepositional = "заказе без отгрузки на долг",
		PrepositionalPlural = "заказах без отгрузки на долги")]
	public class OrderWithoutShipmentForDebt : OrderWithoutShipmentBase, IPrintableRDLDocument, IDocument
	{
		IList<OrderWithoutShipmentForDebtItem> orderWithoutDeliveryForDebtItems = new List<OrderWithoutShipmentForDebtItem>();
		[Display(Name = "Строки заказа без отгрузки на долг")]
		public virtual IList<OrderWithoutShipmentForDebtItem> OrderWithoutDeliveryForDebtItems {
			get => orderWithoutDeliveryForDebtItems;
			set => SetField(ref orderWithoutDeliveryForDebtItems, value);
		}

		GenericObservableList<OrderWithoutShipmentForDebtItem> observableOrderWithoutDeliveryForDebtItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderWithoutShipmentForDebtItem> ObservableOrderWithoutDeliveryForDebtItems {
			get {
				if(observableOrderWithoutDeliveryForDebtItems == null) {
					observableOrderWithoutDeliveryForDebtItems = new GenericObservableList<OrderWithoutShipmentForDebtItem>(orderWithoutDeliveryForDebtItems);
				}

				return observableOrderWithoutDeliveryForDebtItems;
			}
		}
		
		public virtual Email GetEmailAddressForBill()
		{
			return Client.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
		}
		
		#region implemented abstract members of OrderDocument
		public virtual OrderDocumentType Type => OrderDocumentType.BillWithoutShipmentForDebt;
		public virtual Order Order { get; set; }
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = this.Title,
				Identifier = "Documents.BillWithoutShipmentForDebt",
				Parameters = new Dictionary<string, object> {
					{ "order_ws_for_debt_id", Id },
					{ "organization_id", new BaseParametersProvider().GetCashlessOrganisationId },
				}
			};
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public virtual string Title => string.Format($"Счет №{Id} от {CreateDate:d}");

		public virtual string Name => string.Format($"Счет №{Id}");

		public virtual DateTime? DocumentDate => CreateDate;

		public virtual PrinterType PrintType => PrinterType.RDL;
		public virtual DocumentOrientation Orientation => DocumentOrientation.Portrait;

		int copiesToPrint = 1;
		public virtual int CopiesToPrint {
			get => copiesToPrint;
			set => copiesToPrint = value;
		}

		#region Свои свойства

		private bool hideSignature = true;
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
						"тел.: +7(812) 493-50-93\n" +
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
						"<p>тел.: +7 (812) 493-50-93</p>\n" +
						"<p>P.S. И помни, мы тебя любим!</p>\n" +
						"<p>______________</p>\n" +
						"<p>Мы ВКонтакте: <a href=\"https://vk.com/vodovoz_spb\" target=\"_blank\">vk.com/vodovoz_spb</a></p>\n" +
						"<p>Мы в Instagram: @vodovoz_lifestyle</p>\n" +
						"<p>Наш официальный сайт: <a href=\"http://www.vodovoz-spb.ru/\" target=\"_blank\">www.vodovoz-spb.ru</a></p>\n" +
						string.Format("<img src=\"cid:{0}\">", imageId);

			return template;
		}
		
		public OrderWithoutShipmentForDebt() { }
	}

	public interface IDocument
	{
		int Id { get; set; }
		Order Order { get; set; }
		OrderDocumentType Type { get; }
	}
}
