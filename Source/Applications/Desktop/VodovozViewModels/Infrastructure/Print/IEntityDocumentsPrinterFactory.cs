using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.ViewModels.Infrastructure.Print
{
    public interface IEntityDocumentsPrinterFactory
    {
        IEntityDocumentsPrinter CreateOrderDocumentsPrinter(
            Order currentOrder,
            bool? hideSignaturesAndStamps = null,
            IList<OrderDocumentType> orderDocumentTypesToSelect = null);

		IEntityDocumentsPrinter CreateOrderDocumentsPrinter(
			Order currentOrder,
			IDictionary<OrderDocumentType, bool> showSignaturesAndStampsOfDocument);

		IEntityDocumentsPrinter CreateRouteListWithOrderDocumentsPrinter(
	        IUnitOfWork uow,
            RouteList routeList,
	        RouteListPrintableDocuments[] routeListPrintableDocumentTypes,
            IList<OrderDocumentType> orderDocumentTypes = null);
	}
}
