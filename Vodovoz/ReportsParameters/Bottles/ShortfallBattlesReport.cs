using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Employees;
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
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get	{
				return "Отчет о несданных бутылях";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			if(checkReason.Active) {
				if(radiobuttonNewAddress.Active) {
					parameters.Clear();
					parameters.Add("reason", "NewAddress");
				}

				if(radiobuttonOrderIncrease.Active) {
					parameters.Clear();
					parameters.Add("reason", "OrderIncrease");
				}

				if(radiobuttonFirstOrder.Active) {
					parameters.Clear();
					parameters.Add("reason", "FirstOrder");
				}

				if(radiobuttonUnknown.Active) {
					parameters.Clear();
					parameters.Add("reason", "Unknown");
				}
			}
			else
				parameters.Add("reason", -1);

			parameters.Add("date", ydatepicker.Date);

			if(checkOneDriver.Active && yentryDriver.Subject != null)
				parameters.Add("driver_id", (yentryDriver.Subject as Employee).Id);
			else {
				parameters.Add("driver_id", -1);
			}

			switch((Drivers)comboboxDriver.SelectedItem) {
				case Drivers.AllDriver:
					parameters.Add("driver_call", -1);
					break;
				case Drivers.CallFromAnywhere:
					parameters.Add("driver_call", 3);
					break;
				case Drivers.NoCall:
					parameters.Add("driver_call", 2);
					break;
				case Drivers.Largus:
					parameters.Add("driver_call", 1);
					break;
				case Drivers.Hirelings:
					parameters.Add("driver_call", 0);
					break;
				default:
					break;
			}

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

		protected void OnCheckReasonToggled(object sender, EventArgs e)
		{
			var sensitive = checkReason.Active;
			radiobuttonNewAddress.Sensitive = sensitive;
			radiobuttonOrderIncrease.Sensitive = sensitive;
			radiobuttonFirstOrder.Sensitive = sensitive;
			radiobuttonUnknown.Sensitive = sensitive;
		}

		enum Drivers
		{
			[Display(Name = "Все")]
			AllDriver,
			[Display(Name = "Отзвон не с адреса")]
			CallFromAnywhere,
			[Display(Name = "Без отзвона")]
			NoCall,
			[Display(Name = "Ларгусы")]
			Largus,
			[Display(Name = "Наемники")]
			Hirelings
		}
	}
}

