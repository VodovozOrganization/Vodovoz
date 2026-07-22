using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[ToolboxItem(true)]
	public partial class EdoInOrderReceiptSendStageView : WidgetViewBase<EdoInOrderReceiptSendStageViewModel>
	{
		private EdoInOrderReceiptView _receiptView;

		public EdoInOrderReceiptSendStageView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ytreeviewReceipts.ColumnsConfig = FluentColumnsConfig<EdoInOrderReceiptViewModel>.Create()
				.AddColumn("Чеки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DocNumber)
					.XAlign(0.5f)
				.Finish();
			ytreeviewReceipts.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Receipts, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedReceipt, w => w.SelectedRow)
				.InitializeFromSource();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			OpenReceiptView();
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(EdoInOrderReceiptSendStageViewModel.SelectedReceipt))
			{
				OpenReceiptView();
			}
		}

		private void OpenReceiptView()
		{
			if(_receiptView != null)
			{
				_receiptView.Destroy();
				yhboxReceipt.Remove(_receiptView);
			}

			if(ViewModel.SelectedReceipt == null)
			{
				return;
			}

			_receiptView = new EdoInOrderReceiptView();
			_receiptView.ViewModel = ViewModel.SelectedReceipt;
			yhboxReceipt.PackStart(_receiptView, false, true, 0);
			_receiptView.Show();
		}
	}
}
