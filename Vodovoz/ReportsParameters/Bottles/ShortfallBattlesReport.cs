using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModel;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShortfallBattlesReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public ShortfallBattlesReport()
		{
			this.Build();
			ydatepicker.Date = DateTime.Now.Date;
			comboboxDriver.ItemsEnum = typeof(Drivers);
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var filter = new EmployeeFilter(UoW);
			filter.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.driver);
			yentryDriver.RepresentationModel = new EmployeesVM(filter);
			ySpecCmbNonReturnReason.ItemsList = UoW.Session.QueryOver<NonReturnReason>().List();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет о несданных бутылях";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object> {
				{ "reason_id", (ySpecCmbNonReturnReason.SelectedItem as NonReturnReason)?.Id ?? -1 },
				{ "driver_id", (yentryDriver.Subject as Employee)?.Id ?? -1 },
				{ "driver_call", (int)comboboxDriver.SelectedItem },
				{ "date", ydatepicker.Date }
			};

			return new ReportInfo {
				Identifier = "Bottles.ShortfallBattlesReport",
				ParameterDatesWithTime = false,
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateRepotClicked (object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		protected void OnCheckOneDriverToggled(object sender, EventArgs e)
		{
			var sensitive = checkOneDriver.Active;
			yentryDriver.Sensitive = sensitive;
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