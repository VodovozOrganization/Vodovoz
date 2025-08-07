using System;
using System.Globalization;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Service;

namespace Vodovoz.Views.Logistic
{
	public partial class ExportTo1CView : TabViewBase<ExportTo1CViewModel>
	{
		public ExportTo1CView(ExportTo1CViewModel viewModel)
			: base(viewModel)
		{
			Build();

			ConfigureView();
		}

		private void ConfigureView()
		{
			comboOrganization.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CashlessOrganizations, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedCashlessOrganization, w => w.SelectedItem)
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();

			comboRetailOrganization.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RetailOrganizations, w => w.ItemsList)
				.AddBinding(vm => vm.SelectedRetailOrganization, w => w.SelectedItem)
				.AddBinding(vm => vm.CanSaveRetailReport, w => w.Sensitive)
				.InitializeFromSource();

			dateperiodpicker1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonExportBookkeeping.BindCommand(ViewModel.ExportBookkeepingCommand);
			ybuttonComplexAutomation1CExport.BindCommand(ViewModel.ExportComplexAutomationCommand);
			buttonExportIPTinkoff.BindCommand(ViewModel.ExportIPTinkoffCommand);
			ybuttonExportBookkeepingNew.BindCommand(ViewModel.ExportBookkeepingNewCommand);
			buttonSave.BindCommand(ViewModel.SaveFileCommand);
			ybuttonRetailReport.BindCommand(ViewModel.RetailReportCommand);

			labelTotalCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.TotalCounterparty, w => w.Text)
				.InitializeFromSource();

			labelTotalNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.TotalNomenclature, w => w.Text)
				.InitializeFromSource();

			labelTotalSales.Binding
				.AddBinding(ViewModel, vm => vm.TotalSales, w => w.Text)
				.InitializeFromSource();

			labelTotalSum.Binding
				.AddBinding(ViewModel, vm => vm.TotalSum, w => w.Text)
				.InitializeFromSource();

			labelTotalInvoices.Binding
				.AddBinding(ViewModel, vm => vm.TotalInvoices, w => w.Text)
				.InitializeFromSource();

			ViewModel.ExportCompleteAction = OnExportComplete;
		}

		private void OnExportComplete()
		{
			labelExportedSum.Markup =
				$"<span foreground=\"{(ViewModel.ExportData.ExportedTotalSum == ViewModel.ExportData.OrdersTotalSum ? GdkColors.SuccessText.ToHtmlColor() : GdkColors.DangerText.ToHtmlColor())}\">" +
				$"{ViewModel.ExportData.ExportedTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"))}</span>";

			GtkScrolledWindowErrors.Visible = ViewModel.ExportData.Errors.Count > 0;
			if(ViewModel.ExportData.Errors.Count > 0)
			{
				TextTagTable textTags = new TextTagTable();
				var tag = new TextTag("Red");
				tag.Foreground = "red";
				textTags.Add(tag);
				TextBuffer tempBuffer = new TextBuffer(textTags);
				TextIter iter = tempBuffer.EndIter;
				tempBuffer.InsertWithTags(ref iter, String.Join("\n", ViewModel.ExportData.Errors), tag);
				textviewErrors.Buffer = tempBuffer;
			}
		}
	}
}
