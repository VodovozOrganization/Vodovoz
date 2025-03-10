using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.Reports
{
	public partial class ProductionWarehouseMovementReport
	{
		private class ProductionWarehouseMovementReportDataBaseNode
		{
			public DateTime MovementDocumentDate { get; set; }
			public int MovementDocumentId { get; set; }
			public int NomenclatureId { get; set; }
			public string NomenclatureName { get; set; }
			public IList<NomenclaturePurchasePrice> PurchasePrices { get; set; }
			public decimal Amount { get; set; }
		}
	}
}
