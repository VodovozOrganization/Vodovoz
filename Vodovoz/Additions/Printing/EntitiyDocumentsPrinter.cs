using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using Gtk;
using QS.DomainModel.UoW;
using QS.Print;
using QSDocTemplates;
using QSReport;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Additions.Printing
{
	/// <summary>
	/// Класс для составления списка печати документов маршрутного листа, заказа или маршрутного листа и его заказов
	/// с возможностью печати этих документов. Для докуменнтов типа <see cref="QS.Print.PrinterType.ODT"/> производится
	/// автоматический подбор шаблона.
	/// </summary>
	public class EntitiyDocumentsPrinter
	{
		public List<SelectablePrintDocument> DocumentsToPrint { get; set; } = new List<SelectablePrintDocument>();
		public MultipleDocumentPrinter MultiDocPrinter { get; set; }
		public string ODTTemplateNotFoundMessages { get; set; }
		public event EventHandler DocumentsPrinted;
		public event EventHandler PrintingCanceled;
		public static PrintSettings PrinterSettings { get; set; }

		bool? hideSignaturesAndStamps = null;
		bool cancelPrinting = false;
		RouteList currentRouteList;
		IUnitOfWork uow;

		public EntitiyDocumentsPrinter(Order currentOrder, bool? hideSignaturesAndStamps = null, IList<OrderDocumentType> orderDocumentTypesToSelect = null)
		{
			this.hideSignaturesAndStamps = hideSignaturesAndStamps;
			DocPrinterInit();
			FindODTTemplates(currentOrder, orderDocumentTypesToSelect);
		}

		void FindODTTemplates(Order currentOrder, IList<OrderDocumentType> orderDocumentTypesToSelect = null)
		{
			List<string> msgs = null;
			bool? successfulUpdate = null;

			foreach(var item in currentOrder.OrderDocuments) {
				if(item is IPrintableOdtDocument) {
					switch(item.Type) {
						case OrderDocumentType.AdditionalAgreement:
							if((item as IPrintableOdtDocument).GetTemplate() == null)
								successfulUpdate = (item as OrderAgreement).AdditionalAgreement.UpdateContractTemplate(currentOrder.UoW);
							(item as OrderAgreement).PrepareTemplate(currentOrder.UoW);
							break;
						case OrderDocumentType.Contract:
							if((item as IPrintableOdtDocument).GetTemplate() == null)
								successfulUpdate = (item as OrderContract).Contract.UpdateContractTemplate(currentOrder.UoW);
							(item as OrderContract).PrepareTemplate(currentOrder.UoW);
							break;
						case OrderDocumentType.M2Proxy:
							if((item as IPrintableOdtDocument).GetTemplate() == null)
								successfulUpdate = (item as OrderM2Proxy).M2Proxy.UpdateM2ProxyDocumentTemplate(currentOrder.UoW);
							(item as OrderM2Proxy).PrepareTemplate(currentOrder.UoW);
							break;
						default:
							throw new NotImplementedException("Документ не поддерживается");
					}
					if(successfulUpdate == false) {
						if(msgs == null)
							msgs = new List<string>();
						msgs.Add(string.Format("Документ '{0}' в комплект печати добавлен не был, т.к. для него не установлен шаблон документа и не удалось найти подходящий.", item.Name));
						continue;
					}
				}

				if(hideSignaturesAndStamps.HasValue && item is ISignableDocument doc)
					doc.HideSignature = hideSignaturesAndStamps.Value;

				DocumentsToPrint.Add(
					new SelectablePrintDocument(item) {
						Selected = orderDocumentTypesToSelect == null || orderDocumentTypesToSelect != null && orderDocumentTypesToSelect.Contains(item.Type)
					}
				);
			}

			if(msgs != null)
				ODTTemplateNotFoundMessages = string.Join("\n", msgs);
		}

		public EntitiyDocumentsPrinter(IUnitOfWork uow, RouteList routeList, RouteListPrintableDocuments selectedType) : this(uow, routeList, new RouteListPrintableDocuments[] { selectedType })
		{ }

		/// <summary>
		/// Добавление в спсиок печати документов маршрутного листа <paramref name="routeList"/> с выделением типов,
		/// указанных в массиве <paramref name="routeListPrintableDocumentTypes"/>, а также добавление в этот спсиок
		/// документов всех заказов из маршрутного листа <paramref name="routeList"/> с выделением типов, указанных в
		/// массиве <paramref name="orderDocumentTypes"/>. Если <paramref name="orderDocumentTypes"/> не указывать, то
		/// печать документов заказов произведена не будет.
		/// </summary>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="routeList">Маршрутный лист</param>
		/// <param name="routeListPrintableDocumentTypes">Типы документов МЛ, которые необходимо отметить</param>
		/// <param name="orderDocumentTypes">Типы документов заказа, которые необходимо отметить</param>
		public EntitiyDocumentsPrinter(IUnitOfWork uow, RouteList routeList, RouteListPrintableDocuments[] routeListPrintableDocumentTypes, IList<OrderDocumentType> orderDocumentTypes = null)
		{
			this.uow = uow;
			currentRouteList = routeList;
			DocPrinterInit();

			//Эти документы не будут добавлены в список печати вообще
			RouteListPrintableDocuments[] documentsToSkip = {
				RouteListPrintableDocuments.All,
				RouteListPrintableDocuments.LoadSofiyskaya,
				RouteListPrintableDocuments.TimeList,
				RouteListPrintableDocuments.OrderOfAddresses
			};

			foreach(RouteListPrintableDocuments rlDocType in Enum.GetValues(typeof(RouteListPrintableDocuments))) {
				if(!documentsToSkip.Contains(rlDocType)) {
					var rlDoc = new RouteListPrintableDocs(uow, routeList, rlDocType);
					bool isSelected = routeListPrintableDocumentTypes.Contains(RouteListPrintableDocuments.All) || routeListPrintableDocumentTypes.Contains(rlDocType);
					SelectablePrintDocument doc = new SelectablePrintDocument(rlDoc) { Selected = isSelected };
					DocumentsToPrint.Add(doc);
				}
			}
			if(orderDocumentTypes != null)
				PrintOrderDocumentsFromTheRouteList(routeList, orderDocumentTypes);
		}

		void DocPrinterInit()
		{
			MultiDocPrinter = new MultipleDocumentPrinter {
				PrintableDocuments = new GenericObservableList<SelectablePrintDocument>(DocumentsToPrint)
			};
			MultiDocPrinter.DocumentsPrinted += (o, args) => {
				//если среди распечатанных документов есть МЛ, то выставляем его соответствующий признак в true
				if(args is EndPrintArgs endPrintArgs && endPrintArgs.Args.Cast<IPrintableDocument>().Any(d => d.Name == RouteListPrintableDocuments.RouteList.GetEnumTitle())) {
					using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
						var rl = uow.GetById<RouteList>(currentRouteList.Id);
						rl.Printed = true;
						uow.Save(rl);
						uow.Commit();
					}
					uow.Session.Refresh(currentRouteList);
				}
				DocumentsPrinted?.Invoke(o, args);
			};
			MultiDocPrinter.PrintingCanceled += (o, args) => PrintingCanceled?.Invoke(o, args);
		}

		public void Print(SelectablePrintDocument document = null)
		{
			if(!cancelPrinting) {
				MultiDocPrinter.PrinterSettings = PrinterSettings;
				if(document == null)
					MultiDocPrinter.PrintSelectedDocuments();
				else
					MultiDocPrinter.PrintDocument(document);
				PrinterSettings = MultiDocPrinter.PrinterSettings;
			} else {
				PrintingCanceled?.Invoke(this, new EventArgs());
			}
		}

		//для печати документов заказов из МЛ, если есть при печати требуется их печать
		void PrintOrderDocumentsFromTheRouteList(RouteList routeList, IList<OrderDocumentType> orderDocumentTypes)
		{
			var orders = routeList.Addresses
				.Where(a => a.Status != RouteListItemStatus.Transfered)
				.Select(a => a.Order)
				;

			foreach(var o in orders) {
				var orderPrinter = new EntitiyDocumentsPrinter(
					o,
					true,
					//При массовой печати документов заказов из МЛ, в случае наличия у клиента признака UseSpecialDocFields, не будут печататься обычные счета и УПД
					orderDocumentTypes.Where(t => !o.Client.UseSpecialDocFields || t != OrderDocumentType.UPD && t != OrderDocumentType.Bill).ToList()
				);
				orderPrinter.PrintingCanceled += (sender, e) => {
					cancelPrinting = true;
					PrintingCanceled?.Invoke(sender, e);
				};
				ODTTemplateNotFoundMessages = string.Concat(orderPrinter.ODTTemplateNotFoundMessages);
				orderPrinter.Print();
				if(cancelPrinting)
					return;
			}
		}
	}
}