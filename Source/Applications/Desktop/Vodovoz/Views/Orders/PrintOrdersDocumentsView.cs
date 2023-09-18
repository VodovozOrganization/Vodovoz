using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Dialogs.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class PrintOrdersDocumentsView : DialogViewBase<PrintOrdersDocumentsViewModel>
	{
		public PrintOrdersDocumentsView(PrintOrdersDocumentsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonPrint.Clicked += (sender, e) => ViewModel.PrintCommand?.Execute();
			ybuttonPrint.Binding
				.AddBinding(ViewModel, vm => vm.CanPrintDocuments, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonBill.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintBill, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonUpd.Binding
				.AddBinding(ViewModel, vm => vm.IsPrintUpd, w => w.Active)
				.InitializeFromSource();

			ytreeviewOrders.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
				.AddColumn("Заказ").AddTextRenderer(o => o.Id.ToString())
				.AddColumn("Дата").AddTextRenderer(o => o.DeliveryDate.ToString())
				.AddColumn("")
				.Finish();

			ytreeviewOrders.ItemsDataSource = ViewModel.Orders;

			ytreeviewWarnings.HeadersVisible = false;
			ytreeviewWarnings.ColumnsConfig = ColumnsConfigFactory.Create<string>()
				.AddColumn("")
					.AddTextRenderer(x => x)
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.Foreground = "red")
				.Finish();

			ytreeviewWarnings.ItemsDataSource = ViewModel.Warnings;
		}
	}
}
