using QS.Views;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderIncorrectPrices : ViewBase<OrderIncorrectPricesViewModel>
	{
		public OrderIncorrectPrices(OrderIncorrectPricesViewModel viewModel) : base(viewModel)
		{
			this.Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.AddBinding(vm => vm.EntirePeriod, w => w.Sensitive, new BooleanInvertedConverter())
				.InitializeFromSource();

			checkbutton1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.EntirePeriod, w => w.Active)
				.InitializeFromSource();

			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
