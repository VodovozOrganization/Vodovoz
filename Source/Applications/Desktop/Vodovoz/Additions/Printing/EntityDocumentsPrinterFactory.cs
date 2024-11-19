using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.Additions.Printing
{
	public class EntityDocumentsPrinterFactory : IEntityDocumentsPrinterFactory
	{
		public IEntityDocumentsPrinter CreateOrderDocumentsPrinter(
			Order currentOrder, 
			bool? hideSignaturesAndStamps = null, 
			IList<OrderDocumentType> orderDocumentTypesToSelect = null)
		{
			return new EntityDocumentsPrinter(currentOrder, hideSignaturesAndStamps, orderDocumentTypesToSelect);
		}

		public IEntityDocumentsPrinter CreateOrderDocumentsPrinter(
			Order currentOrder,
			IDictionary<OrderDocumentType, bool> showSignaturesAndStampsOfDocument)
		{
			return new EntityDocumentsPrinter(currentOrder, showSignaturesAndStampsOfDocument);
		}

		public IEntityDocumentsPrinter CreateRouteListWithOrderDocumentsPrinter(
			IUnitOfWork uow,
			RouteList routeList,
			RouteListPrintableDocuments[] routeListPrintableDocumentTypes,
			IList<OrderDocumentType> orderDocumentTypes = null)
		{
			return new EntityDocumentsPrinter(uow, routeList, this, routeListPrintableDocumentTypes, orderDocumentTypes);
		}
	}
}
