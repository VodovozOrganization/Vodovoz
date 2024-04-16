using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Store;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using Autofac;
using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Warehouses;
using QS.Dialog.Gtk;
using System.ComponentModel;

namespace Vodovoz.ReportsParameters.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NotFullyLoadedRouteListsReport : SingleUoWWidgetBase, IParametersWidget, INotifyPropertyChanged
	{
		private Warehouse _warehouse;
		private ILifetimeScope _lifetimeScope;
		private readonly INavigationManager _navigationManager;

		public NotFullyLoadedRouteListsReport(ILifetimeScope lifetimeScope, INavigationManager navigationManager)
		{
			Build();
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			datePeriodPicker.StartDate = datePeriodPicker.EndDate = DateTime.Today;
		}

		public void Configure()
		{
			var builder = new LegacyEEVMBuilderFactory<NotFullyLoadedRouteListsReport>(
				DialogHelper.FindParentTab(this), this, UoW, _navigationManager, _lifetimeScope);
			WarehouseEntryViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();

			warehouseEntry.ViewModel = WarehouseEntryViewModel;
		}

		#region IParametersWidget implementation

		public string Title => "Отчет по не полностью погруженным МЛ";

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private IEntityEntryViewModel WarehouseEntryViewModel { get; set; }
		private Warehouse Warehouse { get; set; }



		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		protected void OnDatePeriodPickerPeriodChanged(object sender, EventArgs e)
		{
			SetSensitivity();
		}

		private ReportInfo GetReportInfo()
		{
			var reportInfo = new ReportInfo {
				Identifier = "Store.NotFullyLoadedRouteLists",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", datePeriodPicker.StartDateOrNull.Value },
					{ "end_date", datePeriodPicker.EndDateOrNull.Value },
					{ "warehouse_id", Warehouse?.Id ?? 0}
				}
			};
			return reportInfo;
		}

		private void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		private void SetSensitivity()
		{
			var datePeriodSelected = datePeriodPicker.EndDateOrNull.HasValue && datePeriodPicker.StartDateOrNull.HasValue;
			buttonRun.Sensitive = datePeriodSelected;
		}

		public override void Dispose()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.Dispose();
		}
	}
}
