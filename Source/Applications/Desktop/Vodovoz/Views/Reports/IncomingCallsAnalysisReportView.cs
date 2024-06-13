using Gamma.ColumnConfig;
using QS.Utilities;
using QS.Views.Dialog;
using Vodovoz.EntityRepositories.Contacts;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Views.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomingCallsAnalysisReportView : DialogViewBase<IncomingCallsAnalysisReportViewModel>
	{
		public IncomingCallsAnalysisReportView(IncomingCallsAnalysisReportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			btnFileChooser.Clicked += (sender, args) => ViewModel.ChooseFileCommand.Execute();
			btnReadFile.Clicked += (sender, args) => CreateReport();
			btnHelp.Clicked += (sender, args) => ViewModel.HelpCommand.Execute();
			btnExport.Clicked += (sender, args) => ViewModel.ExportCommand.Execute();
			
			btnFileChooser.Binding
				.AddBinding(ViewModel, vm => vm.SelectedFileTitle, w => w.Label)
				.InitializeFromSource();
			btnReadFile.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateReport, w => w.Sensitive)
				.InitializeFromSource();
			btnExport.Binding
				.AddBinding(ViewModel, vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();
			lblProgress.Binding
				.AddBinding(ViewModel, vm => vm.ProgressTitle, w => w.LabelProp)
				.InitializeFromSource();
			
			ConfigureTree();
		}
		
		private void CreateReport()
		{
			ViewModel.ProgressTitle = ViewModel.LoadingProgress;
			ViewModel.IsLoadingData = true;
			GtkHelper.WaitRedraw();
			ViewModel.CreateReportCommand.Execute();
		}

		private void ConfigureTree()
		{
			treeAnalysis.ColumnsConfig = FluentColumnsConfig<IncomingCallsAnalysisReportNode>.Create()
				.AddColumn("Номер телефона")
					.AddTextRenderer(n => n.PhoneDigitsNumber)
					.XAlign(0.5f)
				.AddColumn("Id клиента")
					.AddTextRenderer(n => ViewModel.ReturnStringFromNullableParameter(n.CounterpartyId))
					.XAlign(0.5f)
				.AddColumn("Id точки доставки")
					.AddTextRenderer(n => ViewModel.ReturnStringFromNullableParameter(n.DeliveryPointId))
					.XAlign(0.5f)
				.AddColumn("№ последнего\nзаказа")
					.AddTextRenderer(n => ViewModel.ReturnStringFromNullableParameter(n.LastOrderId))
					.XAlign(0.5f)
				.AddColumn("Дата последнего\nзаказа")
					.AddTextRenderer(n => ViewModel.ReturnStringFromNullableParameter(n.LastOrderDeliveryDate))
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			treeAnalysis.ItemsDataSource = ViewModel.Nodes;
		}
	}
}
