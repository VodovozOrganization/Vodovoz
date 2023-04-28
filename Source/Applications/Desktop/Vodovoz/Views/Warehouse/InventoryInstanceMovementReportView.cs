using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
	public partial class InventoryInstanceMovementReportView : TabViewBase<InventoryInstanceMovementReportViewModel>
	{
		public InventoryInstanceMovementReportView(InventoryInstanceMovementReportViewModel viewModel) : base(viewModel)
		{
			Build();
		}
	}
}
