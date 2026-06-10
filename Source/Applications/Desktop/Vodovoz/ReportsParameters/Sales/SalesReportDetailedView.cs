using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ReportsParameters;
using Vodovoz.ViewWidgets.Reports;
using VodovozBusiness.Nodes.SalesReport;

namespace Vodovoz.ReportsParameters.Sales
{
	public partial class SalesReportDetailedView : TabViewBase<SalesReportDetailedViewModel>
	{
		public SalesReportDetailedView(SalesReportDetailedViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			dateperiodpicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			/*ycheckbuttonPhones.Binding
				.AddBinding(ViewModel, vm => vm.CanShowPhones, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.ShowPhones, w => w.Active)
				.InitializeFromSource();*/

			ycheckbuttonDetail.Active = true;
			ycheckbuttonDetail.Sensitive = false;

			var filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);

			vboxParameters.Add(filterView);
			filterView.Show();


			ytreeviewDetailedReport.Binding
				.AddBinding(ViewModel, x => x.DisplayNodes, x => x.ItemsDataSource)
				.InitializeFromSource();

			orderdatefilterview1.ViewModel = ViewModel.OrderDateFilterViewModel;
			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			buttonInfo.BindCommand(ViewModel.ShowInfoCommand);
			ybuttonExport.BindCommand(ViewModel.ExportToExcelCommand);

			ConfigureDetailedTreeView();
		}

		private void ConfigureDetailedTreeView()
		{
			var columns = FluentColumnsConfig<SalesReportDisplayNode>.Create()
				.AddColumn("Код")
					.AddTextRenderer(x => x.Code)
				.AddColumn("Клиент")
					.AddTextRenderer(x => x.Counterparty)
					.WrapWidth(200)
					.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Точка доставки")
					.AddTextRenderer(x => x.DeliveryPoint)
					.WrapWidth(250)
					.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Заказ/Дата/Автор")
					.AddTextRenderer(x => x.OrderDetails)
					.WrapWidth(150)
					.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Номенклатура")
					.AddTextRenderer(x => x.Nomenclature)
					.WrapWidth(200)
					.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddNumericRenderer(x => x.Count)
				.AddColumn("Сумма")
					.AddNumericRenderer(x => x.Sum)
					.Digits(2)
				.AddColumn("")
				.Finish();

			ytreeviewDetailedReport.ColumnsConfig = columns;
			ytreeviewDetailedReport.EnableGridLines = TreeViewGridLines.Both;
		}
	}
}
