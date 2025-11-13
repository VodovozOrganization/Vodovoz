using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using Gtk;
using QS.DocTemplates;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Print;
using QSReport;
using Vodovoz.Additions.Logistic;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.Additions.Printing
{
	/// <summary>
	/// Класс для составления списка печати документов маршрутного листа, заказа или маршрутного листа и его заказов
	/// с возможностью печати этих документов. Для докуменнтов типа <see cref="QS.Print.PrinterType.ODT"/> производится
	/// автоматический подбор шаблона.
	/// </summary>
	public class EntityDocumentsPrinter : IEntityDocumentsPrinter
	{
		private readonly IDocTemplateRepository _docTemplateRepository = ScopeProvider.Scope.Resolve<IDocTemplateRepository>();
		private IDictionary<OrderDocumentType, bool> _showSignaturesAndStampsOfDocument;
		private bool? _hideSignaturesAndStamps = null;
		private bool _cancelPrinting = false;

		public event EventHandler DocumentsPrinted;
		public event EventHandler PrintingCanceled;

		public EntityDocumentsPrinter(
			Order currentOrder, 
			bool? hideSignaturesAndStamps = null, 
			IList<OrderDocumentType> orderDocumentTypesToSelect = null)
		{
			_hideSignaturesAndStamps = hideSignaturesAndStamps;
			DocPrinterInit();
			FindODTTemplates(currentOrder, orderDocumentTypesToSelect);
		}

		public EntityDocumentsPrinter(
			Order currentOrder,
			IDictionary<OrderDocumentType, bool> showSignaturesAndStampsOfDocument)
		{
			_showSignaturesAndStampsOfDocument = showSignaturesAndStampsOfDocument
				?? throw new ArgumentNullException(nameof(showSignaturesAndStampsOfDocument));

			DocPrinterInit();
			FindODTTemplates(currentOrder, _showSignaturesAndStampsOfDocument.Keys.ToList());
		}

		/// <summary>
		/// Добавление в спсиок печати документов маршрутного листа <paramref name="routeList"/> с выделением типов,
		/// указанных в массиве <paramref name="routeListPrintableDocumentTypes"/>, а также добавление в этот спсиок
		/// документов всех заказов из маршрутного листа <paramref name="routeList"/> с выделением типов, указанных в
		/// массиве <paramref name="orderDocumentTypes"/>. Если <paramref name="orderDocumentTypes"/> не указывать, то
		/// печать документов заказов произведена не будет.
		/// </summary>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="routeList">Маршрутный лист</param>
		/// <param name="entityDocumentsPrinterFactory">Фабрика принтеров</param>
		/// <param name="routeListPrintableDocumentTypes">Типы документов МЛ, которые необходимо отметить</param>
		/// <param name="orderDocumentTypes">Типы документов заказа, которые необходимо отметить</param>
		public EntityDocumentsPrinter(
			IUnitOfWork uow,
			RouteList routeList,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			RouteListPrintableDocuments[] routeListPrintableDocumentTypes,
			IList<OrderDocumentType> orderDocumentTypes = null)
		{
			if(entityDocumentsPrinterFactory == null)
			{
				throw new ArgumentNullException(nameof(entityDocumentsPrinterFactory));
			}
			
			DocPrinterInit();

			//Эти документы не будут добавлены в список печати вообще
			RouteListPrintableDocuments[] documentsToSkip =
			{
				RouteListPrintableDocuments.All,
				RouteListPrintableDocuments.TimeList,
				RouteListPrintableDocuments.OrderOfAddresses
			};

			foreach(RouteListPrintableDocuments rlDocType in Enum.GetValues(typeof(RouteListPrintableDocuments))) 
			{
				if(!documentsToSkip.Contains(rlDocType)) 
				{
					var rlDoc = new RouteListPrintableDocs(uow, routeList, rlDocType);
					var isSelected = routeListPrintableDocumentTypes.Contains(RouteListPrintableDocuments.All)
									  || routeListPrintableDocumentTypes.Contains(rlDocType);
					
					var doc = new SelectablePrintDocument(rlDoc)
					{
						Selected = isSelected
					};
					
					MultiDocPrinter.PrintableDocuments.Add(doc);
				}
			}

			if(orderDocumentTypes != null)
			{
				PrintOrderDocumentsFromTheRouteList(routeList, entityDocumentsPrinterFactory, orderDocumentTypes);
			}
		}
		
		private MultipleDocumentPrinter MultiDocPrinter { get; set; }
		public static PrintSettings PrinterSettings { get; set; }
		public IList<SelectablePrintDocument> MultiDocPrinterPrintableDocuments => MultiDocPrinter.PrintableDocuments;
		public string ODTTemplateNotFoundMessages { get; set; }

		private void FindODTTemplates(Order currentOrder, IList<OrderDocumentType> orderDocumentTypesToSelect = null)
		{
			List<string> msgs = null;
			bool? successfulUpdate = null;

			foreach(var item in currentOrder.OrderDocuments.OfType<PrintableOrderDocument>()) 
			{
				if(item is IPrintableOdtDocument document) 
				{
					switch(item.Type) 
					{
						case OrderDocumentType.Contract:
							if(document.GetTemplate() == null)
							{
								successfulUpdate =
									(document as OrderContract)?.Contract.UpdateContractTemplate(currentOrder.UoW, _docTemplateRepository);
							}

							(document as OrderContract)?.PrepareTemplate(currentOrder.UoW, _docTemplateRepository);
							break;
						case OrderDocumentType.M2Proxy:
							if(document.GetTemplate() == null)
							{
								successfulUpdate =
									(document as OrderM2Proxy)?.M2Proxy.UpdateM2ProxyDocumentTemplate(currentOrder.UoW, _docTemplateRepository);
							}

							(document as OrderM2Proxy)?.PrepareTemplate(currentOrder.UoW, _docTemplateRepository);
							break;
						case OrderDocumentType.AdditionalAgreement:
							break;
						default:
							throw new NotSupportedException("Документ не поддерживается");
					}

					if(successfulUpdate == false) 
					{
						if(msgs == null)
						{
							msgs = new List<string>();
						}

						msgs.Add($"Документ '{item.Name}' в комплект печати добавлен не был, т.к. для него не установлен шаблон документа и не удалось найти подходящий.");
						continue;
					}
				}

				if(item is ISignableDocument doc)
				{
					if(_hideSignaturesAndStamps.HasValue)
					{
						doc.HideSignature = _hideSignaturesAndStamps.Value;
					}

					if(_showSignaturesAndStampsOfDocument != null
						&& _showSignaturesAndStampsOfDocument.TryGetValue(item.Type, out bool showStampAndSignatureForDocument))
					{
						doc.HideSignature = !showStampAndSignatureForDocument;
					}
				}

				var documentToPrint = new SelectablePrintDocument(item)
				{
					Selected = orderDocumentTypesToSelect == null
								   || orderDocumentTypesToSelect.Contains(item.Type)
				};

				MultiDocPrinter.PrintableDocuments.Add(documentToPrint);
			}

			if(msgs != null)
			{
				ODTTemplateNotFoundMessages = string.Join("\n", msgs);
			}
		}
		
		private void DocPrinterInit()
		{
			MultiDocPrinter = new MultipleDocumentPrinter();

			MultiDocPrinter.DocumentsPrinted += (o, args) => DocumentsPrinted?.Invoke(o, args);
			MultiDocPrinter.PrintingCanceled += (o, args) => PrintingCanceled?.Invoke(o, args);
		}

		//для печати документов заказов из МЛ, если есть при печати требуется их печать
		private void PrintOrderDocumentsFromTheRouteList(
			RouteList routeList,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			IList<OrderDocumentType> orderDocumentTypes)
		{
			var orders = routeList.Addresses
				.Where(a => a.Status != RouteListItemStatus.Transfered)
				.Select(a => a.Order);

			foreach(var order in orders)
			{
				//При массовой печати документов заказов из МЛ, в случае наличия у клиента признака UseSpecialDocFields,
				//не будут печататься обычные счета и УПД
				var orderDocType = orderDocumentTypes
					.Where(t => 
						!order.Client.UseSpecialDocFields || t 
						!= OrderDocumentType.UPD && t 
						!= OrderDocumentType.Bill)
					.ToList();

				if (order.Client.AlwaysPrintInvoice)
				{
					orderDocType.Add(OrderDocumentType.Invoice);
					orderDocType.Add(OrderDocumentType.InvoiceBarter);
					orderDocType.Add(OrderDocumentType.InvoiceContractDoc);
				}

				var orderPrinter = entityDocumentsPrinterFactory.CreateOrderDocumentsPrinter(order, true, orderDocType);				
				orderPrinter.PrintingCanceled += (sender, e) =>
				{
					_cancelPrinting = true;
					PrintingCanceled?.Invoke(sender, e);
				};
				
				ODTTemplateNotFoundMessages = string.Concat(orderPrinter.ODTTemplateNotFoundMessages);
				orderPrinter.Print();

				if(_cancelPrinting)
				{
					return;
				}
			}
		}
		
		public void Print(SelectablePrintDocument document = null)
		{
			if(!_cancelPrinting) 
			{
				MultiDocPrinter.PrinterSettings = PrinterSettings;
				
				if(document == null)
				{
					MultiDocPrinter.PrintSelectedDocuments();
				}
				else
				{
					MultiDocPrinter.PrintDocument(document);
				}

				PrinterSettings = MultiDocPrinter.PrinterSettings;
			} 
			else 
			{
				PrintingCanceled?.Invoke(this, new EventArgs());
			}
		}
	}
}
