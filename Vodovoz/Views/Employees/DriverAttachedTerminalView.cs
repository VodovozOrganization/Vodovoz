using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.Views.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DriverAttachedTerminalView : TabViewBase<DriverAttachedTerminalViewModel>
	{
		public DriverAttachedTerminalView(DriverAttachedTerminalViewModel viewModel) : base(viewModel)
		{
			this.Build();

			labelAuthor.LabelProp = ViewModel.AuthorName;
			labelDate.LabelProp = ViewModel.Date;
			labelDriver.LabelProp = ViewModel.DriverName;
			labelWarehouse.LabelProp = ViewModel.WarehouseTitle;
			labelDocType.LabelProp = ViewModel.DocType;

			labelAuthor.Selectable = true;
			labelDate.Selectable = true;
			labelDriver.Selectable = true;
			labelWarehouse.Selectable = true;

			buttonClose.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
		}
	}
}
