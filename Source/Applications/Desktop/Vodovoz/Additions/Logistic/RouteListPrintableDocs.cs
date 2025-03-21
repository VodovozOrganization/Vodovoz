using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.Additions.Logistic
{
	public class RouteListPrintableDocs : IPrintableRDLDocument
	{
		IUnitOfWork _uow;
		public RouteList routeList;
		RouteListPrintableDocuments _type;

		public RouteListPrintableDocs(IUnitOfWork uow, RouteList routeList, RouteListPrintableDocuments type)
		{
			_uow = uow;
			this.routeList = routeList;
			_type = type;
			CopiesToPrint = DefaultCopies;
		}

		#region IPrintableRDLDocument implementation 
		public ReportInfo GetReportInfo(string connectionString = null) => PrintRouteListHelper.GetRDL(routeList, _type, _uow, IsBatchPrint);
		public Dictionary<object, object> Parameters { get; set; } = new Dictionary<object, object>();
		#endregion

		#region IPrintableDocument implementation 
		public PrinterType PrintType => PrinterType.RDL;

		public string Name => _type.GetEnumTitle();

		public int CopiesToPrint { get; set; }
		#endregion
		
		bool IsBatchPrint => Parameters.ContainsKey("IsBatchPrint") && (bool)Parameters["IsBatchPrint"];
		
		public DocumentOrientation Orientation
		{
			get
			{
				switch(_type)
				{
					case RouteListPrintableDocuments.RouteList:
					case RouteListPrintableDocuments.OrderOfAddresses:
					case RouteListPrintableDocuments.ForwarderReceipt:
					case RouteListPrintableDocuments.ChainStoreNotification:
						return DocumentOrientation.Landscape;
					default:
						return DocumentOrientation.Portrait;
				}
			}
		}

		int DefaultCopies
		{
			get
			{
				switch(_type)
				{
					case RouteListPrintableDocuments.RouteList:
					case RouteListPrintableDocuments.RouteMap:
					case RouteListPrintableDocuments.DailyList:
					case RouteListPrintableDocuments.TimeList:
					case RouteListPrintableDocuments.OrderOfAddresses:
					case RouteListPrintableDocuments.ForwarderReceipt:
					case RouteListPrintableDocuments.ChainStoreNotification:
						return 1;
					default:
						throw new NotImplementedException("Документ не поддерживается");
				}
			}
		}
	}
}
