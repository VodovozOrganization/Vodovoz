using System;
using Gamma.ColumnConfig;
using QS.Print;
using QS.Views.GtkUI;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModels.Accounting;

namespace Vodovoz.Views.Accounting
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class WayBillGeneratorView : TabViewBase<WayBillGeneratorViewModel>
    {
        public WayBillGeneratorView(WayBillGeneratorViewModel viewModel) : base(viewModel)
        {
            this.Build();
            Configure();
        }

        private void Configure()
        {
            ViewModel.StartDate = DateTime.Today.AddDays(-1);
            ViewModel.EndDate = DateTime.Today;

            dateRangeFilter.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
            dateRangeFilter.Binding.AddBinding(ViewModel, vm=> vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();

            entryMechanic.SetEntityAutocompleteSelectorFactory(ViewModel.EntityAutocompleteSelectorFactory);
            entryMechanic.Binding.AddBinding(ViewModel, vm => vm.Mechanic, w => w.Subject);

            yPrintBtn.Clicked += (sender, e) => ViewModel.PrintCommand.Execute();
            yGenerateBtn.Clicked += (sender, e) => ViewModel.GenerateCommand.Execute();
            yUnloadBtn.Clicked += (sender, e) => ViewModel.UnloadCommand.Execute();
        
            yTreeWayBills.ColumnsConfig = FluentColumnsConfig<SelectablePrintDocument>.Create()
                .AddColumn("Печатать")
                    .AddToggleRenderer(x => x.Selected)
                .AddColumn("Дата")
                    .HeaderAlignment(0.5f)
                    .AddTextRenderer(n => (n.Document as WayBillDocument).Date.ToShortDateString())
                .AddColumn("ФИО водителя")
                    .HeaderAlignment(0.5f)
                    .AddTextRenderer(n => (n.Document as WayBillDocument).DriverFIO)
                .AddColumn("Модель машины")
                    .HeaderAlignment(0.5f)
                    .AddTextRenderer(n => (n.Document as WayBillDocument).CarModelName)
                .AddColumn("Расстояние")
                    .HeaderAlignment(0.5f)
                    .AddTextRenderer(n => (n.Document as WayBillDocument).PlanedDistance.ToString())
                .AddColumn("")
                .Finish();
            yTreeWayBills.ItemsDataSource = ViewModel.Entity.WayBillSelectableDocuments;
        }
    }
}
