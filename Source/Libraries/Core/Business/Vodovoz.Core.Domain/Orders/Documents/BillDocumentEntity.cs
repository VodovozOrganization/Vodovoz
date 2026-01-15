using QS.Print;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.StoredEmails;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class BillDocumentEntity : PrintableOrderDocumentEntity
	{
		private int _copiesToPrint = 1;
		private bool _hideSignature = true;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.Bill;
		#endregion

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"Счет №{DocumentOrganizationCounter?.DocumentNumber ?? "-"}"
			:  $"Счет №{Order?.Id}";

		public override DateTime? DocumentDate => Order?.BillDate;

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual CounterpartyEntity Counterparty => Order?.Client;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}

		#region Свои свойства

		/// <summary>
		/// Без подписей и печати
		/// </summary>

		[Display(Name = "Без подписей и печати")]
		public virtual bool HideSignature
		{
			get => _hideSignature;
			set => SetField(ref _hideSignature, value);
		}

		#endregion

		public virtual EmailTemplateEntity GetEmailTemplate(ICounterpartyEdoAccountEntityController edoAccountController = null)
		{
			var template = new EmailTemplateEntity
			{
				Title = "ООО \"Веселый водовоз\"",
				Text =
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
							"Наш официальный сайт: www.vodovoz-spb.ru",
				TextHtml =
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
							"<img src=\"https://cloud1.vod.qsolution.ru/email-attachments/email_ad.png\">"
			};

			return template;
		}

		public virtual EmailTemplateEntity GetResendDocumentEmailTemplate()
		{
			var template = new EmailTemplateEntity
			{
				Title = "ООО \"Веселый водовоз\"",
				Text =
							"Здравствуйте,\n" +
							$"Состав вашего заказа •{Order.Id}• на '{Order.DeliveryDate}' был изменен, для вас сформирован актуальный счет для оплаты заказа.\n" +
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
							$"<p>Состав вашего заказа •{Order.Id}• на '{Order.DeliveryDate}' был изменен, для вас сформирован актуальный счет для оплаты заказа.</p>\n" +
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
