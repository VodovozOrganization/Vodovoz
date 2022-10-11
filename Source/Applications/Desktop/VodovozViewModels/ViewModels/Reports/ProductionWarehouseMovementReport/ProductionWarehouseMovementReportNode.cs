using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.Reports
{
	public partial class ProductionWarehouseMovementReport
	{
		public class ProductionWarehouseMovementReportNode
		{
			public DateTime MovementDocumentDate { get; set; }
			public string MovementDocumentName { get; set; }
			public IList<ProductionWarehouseMovementReportNomenclature> NomenclatureColumns { get; set; }
		}
	}
}