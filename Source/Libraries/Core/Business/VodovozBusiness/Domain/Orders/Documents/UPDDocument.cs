using Autofac;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Globalization;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Controllers;

namespace Vodovoz.Domain.Orders.Documents
{
	public class UPDDocument : PrintableOrderDocument, IPrintableRDLDocument, ICustomResendTemplateEmailableDocument
	{
		private static readonly DateTime _edition2017LastDate =
			Convert.ToDateTime("2021-06-30T23:59:59", CultureInfo.CreateSpecificCulture("ru-RU"));
		private IOrganizationSettings _organizationSettings => ScopeProvider.Scope
			.Resolve<IOrganizationSettings>();

		private IDeliveryScheduleSettings _deliveryScheduleSettings => ScopeProvider.Scope
			.Resolve<IDeliveryScheduleSettings>();

		private int? _beveragesWorldOrganizationId;

		private EmailTemplate GetTemplateForStandartReason(bool hasAgreeForEdo)
		{
			var isFastDelivery = Order.IsFastDelivery;

			var reason = isFastDelivery ? "" : "<br>Т.к. заказ был перенесен на другой маршрут, Вам не привезли закрывающие документы.";

			var fastDeliveryString = isFastDelivery ? "по экспресс-доставке." : "";

			var body = hasAgreeForEdo
				? "Просьба подписать документ в ЭДО или ответным письмом выслать скан с Вашими печатью и подписью"
				: "Просьба ответным письмом выслать скан с Вашими печатью и подписью." +
				  "<br>Если компания использует ЭДО, прошу выслать приглашение по указанным данным ниже, это упростит обмен документами в будущем." +
				  "<br>Наши данные:" +
				  "<br>Оператор ЭДО - ТАКСКОМ" +
				  "<br>ООО \"Веселый Водовоз\" (роуминг, Такском)" +
				  "<br>ИНН 7816453294" +
				  "<br>ИД 2AL-EF740B2F-CA2E-414B-A2A7-F8FA6824B4E4-00000";

			var text = "Добрый день!" +
					   $"<br>" +
					   $"<br>Во вложении {Title} {fastDeliveryString}" +
					   $"{reason}" +
					   $"<br>{body}" +
					   "<br>" +
					   "<br>В случае отказа от обмена через ЭДО, я подготовлю документы для отправки по почте РФ или со следующей поставкой." +
					   "<br>Жду обратной связи.";

			var template = new EmailTemplate
			{
				Title = "ООО \"Веселый водовоз\"",
				TextHtml = text,
				Text = text
			};

			return template;
		}

		private EmailTemplate GetTemplateForClosingDocumentOrder(bool hasAgreeForEdo)
		{
			var body = hasAgreeForEdo
				? "Просьба подписать документ в ЭДО или ответным письмом выслать скан с Вашими печатью и подписью."
				: "Просьба ответным письмом выслать скан с Вашими печатью и подписью." +
				  "<br>Если компания использует ЭДО, прошу выслать приглашение по указанным данным ниже, это упростит обмен документами в будущем." +
				  "<br>Наши данные:" +
				  "<br>Оператор ЭДО - ТАКСКОМ" +
				  "<br>ООО \"Веселый Водовоз\" (роуминг, Такском)" +
				  "<br>ИНН 7816453294" +
				  "<br>ИД 2AL-EF740B2F-CA2E-414B-A2A7-F8FA6824B4E4-00000";

			var text = "Добрый день!" +
					   $"<br>" +
					   $"<br>Во вложении {Title}." +
					   $"<br>{body}" +
					   "<br>" +
					   "<br>В случае отказа от обмена через ЭДО, я подготовлю документы для отправки по почте РФ или со следующей поставкой." +
					   "<br>Жду обратной связи.";

			var template = new EmailTemplate
			{
				Title = "ООО \"Веселый водовоз\"",
				TextHtml = text,
				Text = text
			};

			return template;
		}

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.UPD;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument

		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Identifier = Order.DeliveryDate <= _edition2017LastDate ? "Documents.UPD2017Edition" : "Documents.UPD";
			reportInfo.Title = $"{Name} от {Order.DeliveryDate:d}";
			reportInfo.Parameters = new Dictionary<string, object> {
				{ "order_id", Order.Id },
				{ "special", false },
				{ "hide_signature", HideSignature}
			};
			return reportInfo;
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		#region implemented abstract members of IEmailableDocument

		public virtual string Title => $"{Name} от {Order.DeliveryDate:d}";
		public virtual Counterparty Counterparty => Order?.Client;

		public virtual EmailTemplate GetEmailTemplate(ICounterpartyEdoAccountController edoAccountController = null)
		{
			var hasAgreeForEdo = false;
			
			if(edoAccountController != null)
			{
				var edoAccount = 
					edoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(Order.Client, Order.Contract.Organization.Id);
				hasAgreeForEdo = edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree;
			}

			if( Order.DeliverySchedule.Id == _deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId)
			{
				return GetTemplateForClosingDocumentOrder(hasAgreeForEdo);
			}

			return GetTemplateForStandartReason(hasAgreeForEdo);
		}

		#endregion

		public virtual EmailTemplate GetResendDocumentEmailTemplate()
		{
			var text = $"Добрый день!" +
				$"<br>" +
				$"<br>Во вложении {Title}" +
				$"<br>" +
				$"<br>С Уважением," +
				$"<br>Финансовый отдел" +
				$"<br>Компания \"Веселый Водовоз\"" +
				$"<br>тел.: +7 (812) 317-00-00, доб. 900" +
				$"<br>P.S. И помни, мы тебя любим";

			var template = new EmailTemplate
			{
				Title = "ООО \"Веселый водовоз\"",
				TextHtml = text,
				Text = text
			};

			return template;
		}

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"УПД №{DocumentOrganizationCounter?.DocumentNumber ?? "-"}"
			:  $"УПД №{Order?.Id}";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Landscape;

		public virtual bool HideSignature { get; set; } = true;

		private int copiesToPrint = -1;
		public override int CopiesToPrint
		{
			get
			{
				if(copiesToPrint < 0)
				{
					if(!_beveragesWorldOrganizationId.HasValue)
					{
						_beveragesWorldOrganizationId = _organizationSettings.BeveragesWorldOrganizationId;
					}
					
					if(((Order.OurOrganization != null && Order.OurOrganization.Id == _beveragesWorldOrganizationId)
						|| (Order.Client?.WorksThroughOrganization != null
							&& Order.Client.WorksThroughOrganization.Id == _beveragesWorldOrganizationId))
						&& Order.Client.UPDCount.HasValue)
					{
						return Order.Client.UPDCount.Value;
					}

					return Order.DocumentType.HasValue && Order.DocumentType.Value == DefaultDocumentType.torg12 ? 1 : 2;
				}

				return copiesToPrint;
			}
			set => copiesToPrint = value;
		}
	}
}
