using Autofac;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Domain.Orders.Documents
{
	public class AssemblyListDocument : PrintableOrderDocument, IPrintableRDLDocument
	{
		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.AssemblyList;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			return new ReportInfo {
				Title = string.Format($"Лист сборки от {base.Order.DeliveryDate:d}"),
				Identifier = GetReportName(),
				Parameters = new Dictionary<string, object>
				{
					{ "order_id",  base.Order.Id}
				}
			};
		}

		string GetReportName()
		{
			var nomencltureSettings = ScopeProvider.Scope.Resolve<INomenclatureSettings>();
			var orderItemsQty = Order.OrderItems
				.Count(i => i.Nomenclature.IsFromOnlineShopGroup(nomencltureSettings.IdentifierOfOnlineShopGroup));
			
			return orderItemsQty <= 4 ? "Documents.AssemblyList" : "Documents.SeparateAssemblyList";
		}

		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => string.Format($"Лист сборки от от {Order.DeliveryDate:d}");

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override int CopiesToPrint => 1;
	}
}
