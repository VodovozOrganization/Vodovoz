using System;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using QSReport;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
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
			
			// ViewModel.StartDate = DateTime.Now;
			// ViewModel.EndDate = DateTime.Now + TimeSpan.FromDays(7) + TimeSpan.FromHours(23);
			ViewModel.StartDate = DateTime.Parse("01.07.2020");
			ViewModel.EndDate = DateTime.Parse("02.07.2020");
			dateRangeFilter.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			dateRangeFilter.Binding.AddBinding(ViewModel, vm=> vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();

            entryMechanic.SetEntityAutocompleteSelectorFactory(
                new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
                    () =>
                    {
                        var employeeFilter = new EmployeeFilterViewModel
                        {
                            Status = EmployeeStatus.IsWorking,
                        };
                        return new EmployeesJournalViewModel(
                            employeeFilter,
                            UnitOfWorkFactory.GetDefaultFactory,
                            ServicesConfig.CommonServices);
                    })
            );

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
					.AddTextRenderer(n => (n.Document as WayBillDocument).CarModel)
				.AddColumn("Расстояние")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => (n.Document as WayBillDocument).PlanedDistance.ToString())
				.AddColumn("")
				.Finish();
			yTreeWayBills.ItemsDataSource = ViewModel.Entity.WayBillSelectableDocuments;
			
		}
		
		protected void OnYdatePrintDateChanged(object sender, EventArgs e) => UpdateWayBillList();
		
		void UpdateWayBillList()
		{
			
		}
	}
}
