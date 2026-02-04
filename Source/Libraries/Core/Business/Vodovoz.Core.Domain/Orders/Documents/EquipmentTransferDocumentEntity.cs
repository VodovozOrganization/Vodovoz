using MySqlConnector;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Orders.Documents
{
	public class EquipmentTransferDocumentEntity : PrintableOrderDocumentEntity, IPrintableRDLDocument
	{
		private int _copiesToPrint = 2;

		public EquipmentTransferDocumentEntity()
		{
		}

		#region implemented abstract members of OrderDocument
		public override OrderDocumentType Type => OrderDocumentType.EquipmentTransfer;
		#endregion

		#region implemented abstract members of IPrintableRDLDocument
		public virtual ReportInfo GetReportInfo(string connectionString = null)
		{
			var reportInfoFactory = new DefaultReportInfoFactory(new MySqlConnectionStringBuilder(connectionString));
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = Name;
			reportInfo.Identifier = "Documents.EquipmentTransfer";
			reportInfo.Parameters = new Dictionary<string, object>
			{
				{ "order_id",  Order.Id }
			};
			return reportInfo;
		}
		public virtual Dictionary<object, object> Parameters { get; set; }
		#endregion

		public override string Name => Order?.DeliveryDate >= new DateTime(2026, 1, 1) 
			?  $"АКТ приема-передаточных работ №{DocumentOrganizationCounter?.DocumentNumber ?? Order?.Id.ToString()}"
			:  $"АКТ приема-передаточных работ";

		public override DateTime? DocumentDate => Order?.DeliveryDate;

		public override PrinterType PrintType => PrinterType.RDL;

		public override DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public override int CopiesToPrint
		{
			get => _copiesToPrint;
			set => _copiesToPrint = value;
		}
	}
}
