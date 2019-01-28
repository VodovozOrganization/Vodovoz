using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QSWidgetLib;

namespace Vodovoz.ReportsParameters
{
	public partial class LastOrderByDeliveryPointReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		public LastOrderByDeliveryPointReport()
		{
			this.Build();
			ydatepicker.Date = DateTime.Now.Date;
			BottleDeptEntry.ValidationMode =ValidationType.numeric;
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по последнему заказу";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			bool isSortByBottles;
			int deptCount;
			if(!String.IsNullOrEmpty(BottleDeptEntry.Text)) {
				deptCount = Convert.ToInt32(BottleDeptEntry.Text);
				isSortByBottles = true;
			} 
			else {
				isSortByBottles = false;
				deptCount = 0;
			}

			return new ReportInfo {
				Identifier = buttonSanitary.Active?"Orders.SanitaryReport":"Orders.OrdersByDeliveryPoint",
				Parameters = new Dictionary<string, object>
				{
					{ "date", ydatepicker.Date },
					{ "bottles_count", deptCount},
					{ "is_sort_bottles", isSortByBottles }
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
