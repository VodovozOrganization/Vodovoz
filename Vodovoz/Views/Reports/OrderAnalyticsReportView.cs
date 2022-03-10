using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Utilities;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Views.Reports
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class OrderAnalyticsReportView : DialogViewBase<OrderAnalyticsReportViewModel>
    {
        public OrderAnalyticsReportView(OrderAnalyticsReportViewModel viewModel) : base(viewModel)
        {
            Build();
            Configure();
        }

        private void Configure()
        {
            ConfigureTree();

            ybtnExport.Clicked += (sender, args) => Export();
            ybtnRunReport.Clicked += (sender, args) => RunReport();
            btnHelp.Clicked += (sender, args) => ViewModel.HelpCommand.Execute();

            ybtnRunReport.Binding.AddBinding(ViewModel, vm => vm.HasRunReport, w => w.Sensitive).InitializeFromSource();
            ybtnExport.Binding.AddBinding(ViewModel, vm => vm.HasExportReport, w => w.Sensitive).InitializeFromSource();
            
            ylblProgress.Binding.AddBinding(ViewModel, vm => vm.Progress, w => w.LabelProp).InitializeFromSource();
            
            rangepickerCreationDate.Binding.AddBinding(ViewModel, vm => vm.StartCreationDate, w => w.StartDateOrNull).InitializeFromSource();
            rangepickerCreationDate.Binding.AddBinding(ViewModel, vm => vm.EndCreationDate, w => w.EndDateOrNull).InitializeFromSource();
            rangepickerDeliveryDate.Binding.AddBinding(ViewModel, vm => vm.StartDeliveryDate, w => w.StartDateOrNull).InitializeFromSource();
            rangepickerDeliveryDate.Binding.AddBinding(ViewModel, vm => vm.EndDeliveryDate, w => w.EndDateOrNull).InitializeFromSource();
        }

        private void RunReport()
        {
	        ViewModel.Progress = ViewModel.LoadingData;
	        ViewModel.IsLoadingData = true;
	        GtkHelper.WaitRedraw();
	        ViewModel.RunReportCommand.Execute();
        }
        
        private void Export()
        {
	        var parentW = GtkHelper.GetParentWindow(this);
	        var csvFilter = new FileFilter();
	        csvFilter.AddPattern("*.csv");
	        csvFilter.Name = "Comma Separated Values File (*.csv)";
	        
	        var param = new object[4];
	        param[0] = "Cancel";
	        param[1] = ResponseType.Cancel;
	        param[2] = "Save";
	        param[3] = ResponseType.Accept;

	        var fc = new FileChooserDialog("Save File As", parentW, FileChooserAction.Save, param)
	        {
		        DoOverwriteConfirmation = true,
		        CurrentName = "Аналитика заказов.csv"
	        };
	        
	        fc.AddFilter(csvFilter);

	        if (fc.Run() == (int)ResponseType.Accept)
	        {
		        ViewModel.FileName = fc.Filename;
		        ViewModel.ExportCommand.Execute();
	        }
	        
	        fc.Destroy();
        }

        private void ConfigureTree()
        {
            ytreeviewOrderAnalytics.ColumnsConfig = FluentColumnsConfig<OrderAnalyticsReportNode>.Create()
                .AddColumn("Номер заказа")
                    .AddNumericRenderer(n => n.Id)
                .AddColumn("Номер МЛ")
					.AddTextRenderer(n => 
						!n.RouteListId.HasValue ? string.Empty : n.RouteListId.ToString())
                .AddColumn("ФИО водителя")
                    .AddTextRenderer(n => n.DriverFullName)
                .AddColumn("Статус заказа")
                    .AddTextRenderer(n => n.OrderStatus.GetEnumTitle())
                .AddColumn("Статус адреса")
                    .AddTextRenderer(n => 
						!n.RouteListItemStatus.HasValue ? "" : n.RouteListItemStatus.GetEnumTitle())
                .AddColumn("Модель авто")
                    .AddTextRenderer(n => n.CarModelName)
                .AddColumn("Номер авто")
                    .AddTextRenderer(n => n.CarRegNumber)
                .AddColumn("Тип авто")
                    .AddTextRenderer(n => 
						!n.CarTypeOfUse.HasValue ? string.Empty : n.CarTypeOfUse.GetEnumTitle())
                .AddColumn("Принадлежность авто")
					.AddTextRenderer(n => !n.CarOwnType.HasValue ? string.Empty : n.CarOwnType.GetEnumTitle())
                .AddColumn("Разовый водитель?")
                    .AddTextRenderer(n => 
						!n.IsDriverForOneDay.HasValue ? string.Empty : n.IsDriverForOneDay.Value ? "Да" : "Нет")
                .AddColumn("19л бутылей")
                    .AddNumericRenderer(n => n.Bottles19LCount)
                .AddColumn("Адрес")
                    .AddTextRenderer(n => n.Address)
                .AddColumn("Район")
                    .AddTextRenderer(n => n.DistrictName)
                .AddColumn("Часть города")
                    .AddTextRenderer(n => n.GeographicGroupName)
                .AddColumn("Город/Пригород")
                    .AddTextRenderer(n => n.CityOrSuburb ?? "Неизвестно")
                .AddColumn("Интервал от")
                    .AddTextRenderer(n => n.DeliveryScheduleFrom.ToString())
                .AddColumn("Интервал до")
                    .AddTextRenderer(n => n.DeliveryScheduleTo.ToString())
                .AddColumn("Дата доставки")
                    .AddTextRenderer(n => n.DeliveryDate.ToShortDateString())
                .AddColumn("Дата создания заказа")
                    .AddTextRenderer(n => n.CreationDate.ToString())
                .AddColumn("ЗП водителя за адрес")
                    .AddNumericRenderer(n => n.DriverWage)
                    .Digits(2)
                .AddColumn("ФИО экспедитора")
                    .AddTextRenderer(n => 
                        string.IsNullOrEmpty(n.ForwarderLastName) ? "Без экспедитора" : n.ForwarderFullName)
                .AddColumn("Общая сумма заказа")
                    .AddNumericRenderer(n => n.OrderSum)
                    .Digits(2)
                .AddColumn("Сумма за 19л бутыли")
                    .AddNumericRenderer(n => n.Bottles19LSum)
                    .Digits(2)
                .AddColumn("Cредняя стоимость 19л в заказе")
                    .AddNumericRenderer(n => n.Bottles19LAvgPrice)
                    .Digits(2)
                .AddColumn("")
                .Finish();

            ytreeviewOrderAnalytics.ItemsDataSource = ViewModel.NodesList;
        }
    }
}
