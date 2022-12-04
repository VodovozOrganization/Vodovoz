using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using QS.Dialog.GtkUI;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using QS.Navigation;
using Vodovoz.Core.Domain.Employees;
using QS.Project.Services;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShortfallBattlesReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly INavigationManager _navigationManager;
		private readonly ReportFactory _reportFactory;

		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			this.Build();
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			ydatepicker.Date = DateTime.Now.Date;
			comboboxDriver.ItemsEnum = typeof(Drivers);
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
			var filter = new EmployeeFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			var driverFactory = new EmployeeJournalFactory(_navigationManager, filter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(driverFactory.CreateEmployeeAutocompleteSelectorFactory());
			ySpecCmbNonReturnReason.ItemsList = UoW.Session.QueryOver<NonReturnReason>().List();
			buttonCreateRepot.Clicked += (s, a) => OnUpdate(true);
			checkOneDriver.Toggled += OnCheckOneDriverToggled;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет о несданных бутылях";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object> {
				{ "reason_id", (ySpecCmbNonReturnReason.SelectedItem as NonReturnReason)?.Id ?? -1 },
				{ "driver_id", (evmeDriver.Subject as Employee)?.Id ?? -1 },
				{ "driver_call", (int)comboboxDriver.SelectedItem },
				{ "date", ydatepicker.Date }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Bottles.ShortfallBattlesReport";
			reportInfo.ParameterDatesWithTime = false;
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnCheckOneDriverToggled(object sender, EventArgs e)
		{
			var sensitive = checkOneDriver.Active;
			evmeDriver.Sensitive = sensitive;
		}

		enum Drivers
		{
			[Display(Name = "Все")]
			AllDriver = -1,
			[Display(Name = "Отзвон не с адреса")]
			CallFromAnywhere = 3,
			[Display(Name = "Без отзвона")]
			NoCall = 2,
			[Display(Name = "Ларгусы")]
			Largus = 1,
			[Display(Name = "Наемники")]
			Hirelings = 0
		}
	}
}
