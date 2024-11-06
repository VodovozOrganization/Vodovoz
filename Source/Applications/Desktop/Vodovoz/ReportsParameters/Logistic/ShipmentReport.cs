using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShipmentReport : ViewBase<ShipmentReportViewModel>, ISingleUoWDialog
	{
		public ShipmentReport(ShipmentReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydatepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			radioAll.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.All, w => w.Active)
				.InitializeFromSource();

			radioCash.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SortByCash, w => w.Active)
				.InitializeFromSource();

			radioWarehouse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SortByWarehouse, w => w.Active)
				.InitializeFromSource();

			//Необходимо Заменить на новый entry
			referenceWarehouse.ItemsQuery = StoreDocumentHelper.GetNotArchiveWarehousesQuery();
			referenceWarehouse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Warehouse, w => w.Subject)
				.AddBinding(vm => vm.SortByWarehouse, w => w.Sensitive)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}
