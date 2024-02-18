﻿using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Organizations;

namespace Vodovoz.Domain.Orders.OrdersWithoutShipment
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "счета без отгрузки на долги",
		Nominative = "счет без отгрузки на долг",
		Prepositional = "счете без отгрузки на долг",
		PrepositionalPlural = "счетах без отгрузки на долги")]
	[EntityPermission]
	[HistoryTrace]
	public class OrderWithoutShipmentForDebt : OrderWithoutShipmentBase, IPrintableRDLDocument, IEmailableDocument, IValidatableObject
	{
		public virtual int Id { get; set; }
		
		private decimal debtSum;
		[Display(Name = "Сумма долга")]
		public virtual decimal DebtSum {
			get => debtSum;
			set
			{
				if(SetField(ref debtSum, value))
					RecalculateNDS();
			}
		}
		
		decimal includeNDS;
		[Display(Name = "Включая НДС")]
		public virtual decimal IncludeNDS {
			get => includeNDS;
			set => SetField(ref includeNDS, value);
		}

		private decimal? valueAddedTax = 0.20m;
		[Display(Name = "НДС на момент создания")]
		public virtual decimal? ValueAddedTax {
			get => valueAddedTax;
			set => SetField(ref valueAddedTax, value);
		}

		private string debtName = "Задолженность по акту сверки";
		[Display(Name = "Наименование задолженности")]
		public virtual string DebtName {
			get => debtName;
			set => SetField(ref debtName, value);
		}
		
		void RecalculateNDS()
		{
			IncludeNDS = Math.Round(DebtSum * ValueAddedTax.Value / (1 + ValueAddedTax.Value), 2);
		}
		
		public virtual OrderDocumentType Type => OrderDocumentType.BillWSForDebt;

		private Order order;
		public virtual Order Order {
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
			return new ReportInfo
			{
				Title = this.Title,
				Identifier = "Documents.BillWithoutShipmentForDebt",
				Parameters = new Dictionary<string, object> {
					{ "bill_ws_for_debt_id", Id },
					{ "special_contract_number", SpecialContractNumber },
					{ "organization_id", settings.GetCashlessOrganisationId },
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

			if (DebtSum == 0)
				yield return new ValidationResult(
					"Сумма долга не может быть равна нулю.",
					new[] {nameof(DebtSum)}
				);
			
			if (string.IsNullOrEmpty(DebtName))
				yield return new ValidationResult(
					"Наименование задолженности должно быть заполнено.",
					new[] {nameof(DebtName)}
				);
		}
	}
}
