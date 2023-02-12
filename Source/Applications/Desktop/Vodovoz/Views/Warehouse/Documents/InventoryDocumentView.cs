using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Warehouses.Documents;

namespace Vodovoz.Views.Warehouse.Documents
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InventoryDocumentView : TabViewBase<InventoryDocumentViewModel>
	{
		public InventoryDocumentView(InventoryDocumentViewModel viewModel)
			: base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{

		}
	}
}
