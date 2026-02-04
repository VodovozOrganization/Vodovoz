using Gtk;
using QS.Views.GtkUI;
using QS.Widgets;
using System;
using System.Globalization;
using QS.Views.Dialog;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.Service;

namespace Vodovoz.Views.Logistic
{
	public partial class ExportTo1CView : DialogViewBase<ExportTo1CViewModel>
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
				.AddBinding(vm => vm.CanExport, w => w.Sensitive)
				.InitializeFromSource();

			dateperiodpicker1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonExportBookkeeping.BindCommand(ViewModel.ExportCashlessBookkeepingCommand);
			ybuttonComplexAutomation1CExport.BindCommand(ViewModel.ExportCashlessComplexAutomationCommand);
			buttonExportIPTinkoff.BindCommand(ViewModel.ExportCashlessIPTinkoffCommand);
			ybuttonExportBookkeepingNew.BindCommand(ViewModel.ExportCashlessBookkeepingNewCommand);
			buttonSave.BindCommand(ViewModel.SaveExportCashlessDataCommand);
			ybuttonRetailReport.BindCommand(ViewModel.RetailReportCommand);
			ybuttonRetailExport.BindCommand(ViewModel.ExportRetailCommand);

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

			progresswidget.Visible = false;
			ViewModel.ProgressBarDisplayable = progresswidget;

			ViewModel.ExportCompleteAction = OnExportComplete;

			ViewModel.StartProgressAction = OnStartProgress;
			ViewModel.EndProgressAction = OnEndProgress;			
		}

		private void OnEndProgress()
		{
			progresswidget.Visible = false;
		}

		private void OnStartProgress()
		{
			progresswidget.Visible = true;
		}

		private void OnExportComplete()
		{
			labelExportedSum.Markup =
				$"<span foreground=\"{(ViewModel.ExportCashlessData.ExportedTotalSum == ViewModel.ExportCashlessData.OrdersTotalSum ? GdkColors.SuccessText.ToHtmlColor() : GdkColors.DangerText.ToHtmlColor())}\">" +
				$"{ViewModel.ExportCashlessData.ExportedTotalSum.ToString("C", CultureInfo.GetCultureInfo("ru-RU"))}</span>";

			GtkScrolledWindowErrors.Visible = ViewModel.ExportCashlessData.Errors.Count > 0;
			if(ViewModel.ExportCashlessData.Errors.Count > 0)
			{
				TextTagTable textTags = new TextTagTable();
				var tag = new TextTag("Red");
				tag.Foreground = "red";
				textTags.Add(tag);
				TextBuffer tempBuffer = new TextBuffer(textTags);
				TextIter iter = tempBuffer.EndIter;
				tempBuffer.InsertWithTags(ref iter, String.Join("\n", ViewModel.ExportCashlessData.Errors), tag);
				textviewErrors.Buffer = tempBuffer;
			}
		}
	}
}
