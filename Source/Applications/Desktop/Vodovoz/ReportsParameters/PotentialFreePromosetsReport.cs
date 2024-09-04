using Gamma.ColumnConfig;
using QS.Views;
using System.ComponentModel;
using Vodovoz.ViewModels.ReportsParameters.Sales;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class PotentialFreePromosetsReport : ViewBase<PotentialFreePromosetsReportViewModel>
	{
		public PotentialFreePromosetsReport(PotentialFreePromosetsReportViewModel viewModel) : base(viewModel)
		{
			Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ytreeview1.ColumnsConfig = FluentColumnsConfig<PromosetReportNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.Active)
				.AddColumn("Промонабор").AddTextRenderer(x => x.Name)
				.Finish();
			ytreeview1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Promosets, w => w.ItemsDataSource)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
