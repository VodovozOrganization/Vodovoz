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

			ycheckbuttonPhones.Binding
				.AddBinding(ViewModel, vm => vm.CanShowPhones, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.ShowPhones, w => w.Active)
				.InitializeFromSource();

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

			buttonInfo.BindCommand(ViewModel.ShowInfoCommand);

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			buttonCreateReport.Binding
				.AddBinding(ViewModel, vw => vw.ReportIsNotLoaded, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonExport.BindCommand(ViewModel.ExportToExcelCommand);
			ybuttonExport.Binding
				.AddBinding(ViewModel, vw => vw.ReportIsNotExported, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureDetailedTreeView();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void ConfigureDetailedTreeView()
		{
			var config = FluentColumnsConfig<SalesReportDisplayNode>.Create();

			config.AddColumn("Код")
				.AddTextRenderer(x => x.Code);

			config.AddColumn("Клиент")
				.AddTextRenderer(x => x.Counterparty)
				.WrapWidth(200)
				.WrapMode(Pango.WrapMode.WordChar);

			config.AddColumn("Точка доставки")
				.AddTextRenderer(x => x.DeliveryPoint)
				.WrapWidth(250)
				.WrapMode(Pango.WrapMode.WordChar);

			config.AddColumn("Заказ/Дата/Автор")
				.AddTextRenderer(x => x.OrderDetails)
				.WrapWidth(150)
				.WrapMode(Pango.WrapMode.WordChar);

			if(ViewModel.ShowPhones)
			{
				config.AddColumn("Телефоны")
					.AddTextRenderer(x => x.Phones)
					.WrapWidth(120)
					.WrapMode(Pango.WrapMode.WordChar);
			}

			config.AddColumn("Номенклатура")
				.AddTextRenderer(x => x.Nomenclature)
				.WrapWidth(200)
				.WrapMode(Pango.WrapMode.WordChar);

			config.AddColumn("Кол-во")
				.AddNumericRenderer(x => x.Count);

			config.AddColumn("Сумма")
				.AddNumericRenderer(x => x.Sum)
				.Digits(2);

			config.AddColumn("");

			ytreeviewDetailedReport.ColumnsConfig = config.Finish();
			ytreeviewDetailedReport.EnableGridLines = TreeViewGridLines.Both;
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.ShowPhones))
			{
				ApplyPhoneColumnVisibility();
			}
		}

		private void ApplyPhoneColumnVisibility()
		{
			if(ytreeviewDetailedReport?.ColumnsConfig is null)
			{
				return;
			}

			ConfigureDetailedTreeView();
			ytreeviewDetailedReport.QueueDraw();
		}

		public override void Destroy()
		{
			base.Destroy();

			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
		}
	}
}
