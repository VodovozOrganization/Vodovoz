using Gamma.ColumnConfig;
using QS.Navigation;
using QS.Utilities;
using QS.Views.Dialog;
using Vodovoz.Domain.Payments;
using Vodovoz.ViewModels;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ImportPaymentsFromAvangardSbpView : DialogViewBase<ImportPaymentsFromAvangardSbpViewModel>
	{
		public ImportPaymentsFromAvangardSbpView(ImportPaymentsFromAvangardSbpViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnFileChooser.Clicked += (sender, args) => ViewModel.ChooseFileCommand.Execute();
			btnReadFile.Clicked += (sender, args) => ParsePaymentRegistry();
			btnHelp.Clicked += (sender, args) => ViewModel.HelpCommand.Execute();
			btnLoad.Clicked += (sender, args) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);

			btnLoad.Binding
				.AddBinding(ViewModel, vm => vm.CanLoad, w => w.Sensitive)
				.InitializeFromSource();
			btnFileChooser.Binding
				.AddBinding(ViewModel, vm => vm.SelectedFileTitle, w => w.Label)
				.InitializeFromSource();
			btnReadFile.Binding
				.AddBinding(ViewModel, vm => vm.CanParsePayments, w => w.Sensitive)
				.InitializeFromSource();
			lblProgress.UseMarkup = true;
			lblProgress.Binding
				.AddBinding(ViewModel, vm => vm.ProgressTitle, w => w.LabelProp)
				.InitializeFromSource();

			ConfigureTree();
		}
		
		private void ParsePaymentRegistry()
		{
			ViewModel.ProgressTitle = ViewModel.ParsingProgress;
			ViewModel.IsParsingData = true;
			GtkHelper.WaitRedraw();
			ViewModel.ParsePaymentRegistryCommand.Execute();
		}

		private void ConfigureTree()
		{
			treeData.ColumnsConfig = FluentColumnsConfig<PaymentFromAvangard>.Create()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => $"{ViewModel.AvangardPayments.IndexOf(n) + 1}")
					.XAlign(0.5f)
				.AddColumn("Дата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.PaidDate.ToString())
					.XAlign(0.5f)
				.AddColumn("№ заказа")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.OrderNum.ToString())
					.XAlign(0.5f)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.TotalSum.ToShortCurrencyString())
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			treeData.ItemsDataSource = ViewModel.AvangardPayments;
		}
	}
}
