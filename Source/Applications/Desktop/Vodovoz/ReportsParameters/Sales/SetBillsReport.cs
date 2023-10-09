using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QS.ViewModels.Control.EEVM;
using QSReport;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SetBillsReport : SingleUoWWidgetBase, IParametersWidget
	{
		public SetBillsReport(IUnitOfWorkFactory unitOfWorkFactory)
		{
			Build();

			UoW = unitOfWorkFactory.CreateWithoutRoot();
			
			daterangepickerOrderCreation.StartDate = DateTime.Now;
			daterangepickerOrderCreation.EndDate = DateTime.Now;
			ybuttonCreateReport.Clicked += (sender, e) => { OnUpdate(true); };
			ybuttonCreateReport.TooltipText = $"Формирует отчет по заказам в статусе '{OrderStatus.WaitForPayment.GetEnumTitle()}'";

		}

		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по выставленным счетам";

		#endregion

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Sales.SetBillsReport",
				Parameters = new Dictionary<string, object>
				{
					{ "creationDate", DateTime.Now },
					{ "startDate", daterangepickerOrderCreation.StartDate.Date },
					{ "endDate", daterangepickerOrderCreation.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59) },
					{ "authorSubdivision", (entrySubdivision.Subject as Subdivision)?.Id }
				}
			};
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}
	}
}
