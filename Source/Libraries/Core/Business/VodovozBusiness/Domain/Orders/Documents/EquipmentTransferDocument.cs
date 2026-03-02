using Autofac;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Controllers;

namespace Vodovoz.Domain.Orders.Documents
{
	/// <summary>
	/// Акт приема-передачи оборудования
	/// </summary>
	public class EquipmentTransferDocument : PrintableOrderDocument, IPrintableRDLDocument, IEmailableDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.EquipmentTransfer;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = Name;
			reportInfo.Identifier = "Documents.EquipmentTransfer";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id",  Order.Id }
			};
			return reportInfo;
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"Акт приема-передачи оборудования №{DocumentOrganizationCounter?.DocumentNumber ?? Order?.Id.ToString()}"
			:  $"Акт приема-передачи оборудования";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		private int _copiesToPrint = 2;
		public override int CopiesToPrint {
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}

		public virtual string Title => $"Акт приема-передачи оборудования №{Order.Id} от {Order.DeliveryDate:d}";

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty => Order?.Client;

		private bool _hideSignature = true;

		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature
		{
			get => _hideSignature;
			set => SetField(ref _hideSignature, value, () => HideSignature);
		}

		public virtual EmailTemplate GetEmailTemplate(ICounterpartyEdoAccountController edoAccountController = null, IOrganizationSettings organizationSettings = null)
		{
			var template = new EmailTemplate
			{
				Title = "ООО \"Веселый водовоз\"",
				Text =
							"Здравствуйте,\n" +
							"Для Вас сформирован акт приёма-передачи оборудования.\n" +
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
							"Наш официальный сайт: www.vodovoz-spb.ru",
				TextHtml =
							"<p>Здравствуйте</p>\n" +
							"<p>Для Вас сформирован акт приёма-передачи оборудования.</p>\n" +
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
							"<img src=\"https://cloud1.vod.qsolution.ru/email-attachments/email_ad.png\">"
			};

			return template;
		}

		public virtual EmailTemplate GetResendEmailTemplate()
		{
			var template = new EmailTemplate
			{
				Title = "ООО \"Веселый водовоз\"",
				Text =
							"Здравствуйте,\n" +
							$"Состав вашего заказа •{Order.Id}• на '{Order.DeliveryDate}' был изменен, для вас сформирован актуальный акт приёма-передачи оборудования.\n" +
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
							"Наш официальный сайт: www.vodovoz-spb.ru",
				TextHtml =
							"<p>Здравствуйте</p>\n" +
							$"<p>Состав вашего заказа •{Order.Id}• на '{Order.DeliveryDate}' был изменен, для вас сформирован актуальный акт приёма-передачи оборудования.</p>\n" +
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
							"<img src=\"https://cloud1.vod.qsolution.ru/email-attachments/email_ad.png\">"
			};

			return template;
		}
	}
}

