﻿using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Dialog;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SendedEmailsReport : SingleUoWWidgetBase, IParametersWidget
	{
		public SendedEmailsReport()
		{
			this.Build();
			ydateperiodpicker.StartDate = DateTime.Now;
			ydateperiodpicker.EndDate = DateTime.Now;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по отправке счетов";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Orders.SendedBillsReport",
				Parameters = new Dictionary<string, object>
				{
					{ "startDate", ydateperiodpicker.StartDate },
					{ "endDate", ydateperiodpicker.EndDate }
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}
	}
}
