using System;
using System.Collections.Generic;
using Autofac;
using QS.Print;
using QS.Report;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using VodovozBusiness.Controllers;

namespace Vodovoz.Domain.Orders.Documents
{
	/// <summary>
	/// Документ письма о задолженности
	/// </summary>
	public class LetterOfDebtDocument : PrintableOrderDocument, IPrintableRDLDocument, ISignableDocument, IEmailableDocument
	{
		private int _copiesToPrint = 2;
		private bool _hideSignature = true;

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.LetterOfDebt;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = Name;
			reportInfo.Identifier = "Documents.LetterOfDebt";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id",  Order.Id },
				{ "hide_signature", HideSignature }
			};
			return reportInfo;
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => "Письмо о задолженности";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public override int CopiesToPrint 
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}

		public virtual bool HideSignature
		{
			get => _hideSignature;
			set => _hideSignature = value;
		}

		public virtual EmailTemplate GetEmailTemplate(ICounterpartyEdoAccountController edoAccountController = null)
		{
			string deliveryDateFormatted = Order.DeliveryDate?.ToString("dd.MM.yyyy")
				?? string.Empty;

			string dueDateFormatted = Order.DeliveryDate?.AddDays(Order.Client.DelayDaysForBuyers).ToString("dd.MM.yyyy") ?? string.Empty;

			var template = new EmailTemplate
			{
				Title = "ООО \"Веселый водовоз\" - Задолженность по заказу",
				Text =
					"Уважаемый клиент!\n\n" +
					$"Мы хотим напомнить вам, что на текущий момент у вас имеется задолженность по заказу №{Order.Id} от {deliveryDateFormatted},\n" +
					$"срок оплаты по которому истёк {dueDateFormatted}.\n\n" +
					"Мы ценим ваше сотрудничество и надеемся на оперативное урегулирование задолженности.\n" +
					"Для уточнения деталей Вы можете связаться с нами по телефону 8 812-317-00-00 доб. 900\n" +
					"или электронной почте client.buh@vodovoz-spb.ru.\n\n" +
					"Просим вас принять меры к погашению задолженности в ближайшее время, чтобы избежать возможных последствий,\n" +
					"включая, при необходимости, обращение в суд.\n\n" +
					"Благодарим за внимание и надеемся на скорое решение вопроса.\n\n" +
					"Подробная информация о задолженности приложена к письму в формате PDF.\n" +
					"Ссылка для отписки от рассылки: {unsubscribeUrl}\n" +
					"Вы всегда можете отписаться от нашей рассылки.\n" +
					"Это письмо отправлено автоматически.",
				TextHtml = $@"
					<!DOCTYPE html>
					<html>
					<head>
						<meta charset='utf-8'>
						<style>
							body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
							.container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
							.header {{ background-color: #f8f9fa; padding: 20px; border-radius: 5px; }}
							.content {{ margin: 20px 0; }}
							.footer {{ margin-top: 30px; padding-top: 15px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
							.unsubscribe {{ color: #007bff; text-decoration: none; }}
						</style>
					</head>
					<body>
						<div class='container'>
							<div class='header'>
								<h2>Уважаемый клиент!</h2>
							</div>
        
							<div class='content'>
								<p>Мы хотим напомнить вам, что на текущий момент у вас имеется задолженность по заказу №{Order.Id} от {deliveryDateFormatted},
								срок оплаты по которому истёк <strong>{dueDateFormatted}.</strong></p>

								<p>Мы ценим ваше сотрудничество и надеемся на оперативное урегулирование задолженности. 
								Для уточнения деталей Вы можете связаться с нами по телефону <strong>8 812-317-00-00 доб. 900</strong> 
								или электронной почте <strong>client.buh@vodovoz-spb.ru</strong>.</p>

								<p>Просим вас принять меры к погашению задолженности в ближайшее время, чтобы избежать возможных последствий, 
								включая, при необходимости, обращение в суд.</p>

								<p>Благодарим за внимание и надеемся на скорое решение вопроса.</p>
							</div>
        
							<div class='footer'>
								<p>Подробная информация о задолженности приложена к письму в формате PDF.</p>
								<p>Вы всегда можете отписаться от нашей рассылки, нажав соответствующую кнопку.</p>
								<p><em>Это письмо отправлено автоматически.</em></p>
							</div>
						</div>
					</body>
					</html>"
			};

			return template;
		}

		public virtual string Title => $"Письмо о задолженности №{Order.Id} от {DocumentDate?.ToString("dd.MM.yyyy")}";

		
		public virtual Counterparty Counterparty => Order.Client;
	}
}

