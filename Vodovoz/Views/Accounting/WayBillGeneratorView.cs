using System;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using QS.Utilities;
using QS.Views.GtkUI;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModels.Accounting;
using Vodovoz.ViewModels.Client;
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
			
            //entryMechanic

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
