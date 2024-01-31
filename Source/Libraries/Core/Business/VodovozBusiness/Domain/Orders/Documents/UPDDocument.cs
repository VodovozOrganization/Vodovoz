using System;
using System.Collections.Generic;
using System.Globalization;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Domain.Orders.Documents
{
	public class UPDDocument : PrintableOrderDocument, IPrintableRDLDocument, IEmailableDocument
	{
		private static readonly DateTime _edition2017LastDate =
			Convert.ToDateTime("2021-06-30T23:59:59", CultureInfo.CreateSpecificCulture("ru-RU"));
		private static readonly IOrganizationParametersProvider _organizationParametersProvider =
			new OrganizationParametersProvider(new ParametersProvider());
		private readonly IDeliveryScheduleParametersProvider _deliveryScheduleParametersProvider =
			new DeliveryScheduleParametersProvider(new ParametersProvider());
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
					   $"<br>Во вложении {Title} по сервиному обслуживанию" +
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
			var identifier = Order.DeliveryDate <= _edition2017LastDate ? "Documents.UPD2017Edition" : "Documents.UPD";
			return new ReportInfo {
				Title = $"УПД {Order.Id} от {Order.DeliveryDate:d}",
				Identifier = identifier,
				Parameters = new Dictionary<string, object> {
					{ "order_id", Order.Id },
					{ "special", false },
					{ "hide_signature", HideSignature}
				},
				RestrictedOutputPresentationTypes = RestrictedOutputPresentationTypes
			};
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		#region implemented abstract members of IEmailableDocument

		public virtual string Title => $"УПД №{Order.Id} от {Order.DeliveryDate:d}";
		public virtual Counterparty Counterparty => Order?.Client;

		public virtual EmailTemplate GetEmailTemplate()
		{
			var hasAgreeForEdo = Order.Client.ConsentForEdoStatus == ConsentForEdoStatus.Agree;

			if( Order.DeliverySchedule.Id == _deliveryScheduleParametersProvider.ClosingDocumentDeliveryScheduleId)
			{
				return GetTemplateForClosingDocumentOrder(hasAgreeForEdo);
			}

			return GetTemplateForStandartReason(hasAgreeForEdo);
		}

		#endregion

		public override string Name => $"УПД №{Order.Id}";

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
						_beveragesWorldOrganizationId = _organizationParametersProvider.BeveragesWorldOrganizationId;
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
