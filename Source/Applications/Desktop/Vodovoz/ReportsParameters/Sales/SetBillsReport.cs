using System;
using System.Collections.Generic;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Orders;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SetBillsReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly ReportFactory _reportFactory;

		public SetBillsReport(
			ReportFactory reportFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEntityAutocompleteSelectorFactory subdivisionSelectorFactory)
		{
			if(subdivisionSelectorFactory == null)
			{
				throw new ArgumentNullException(nameof(subdivisionSelectorFactory));
			}
			
			Build();

			UoW = unitOfWorkFactory.CreateWithoutRoot();
			
			daterangepickerOrderCreation.StartDate = DateTime.Now;
			daterangepickerOrderCreation.EndDate = DateTime.Now;
			ybuttonCreateReport.Clicked += (sender, e) => { OnUpdate(true); };
			ybuttonCreateReport.TooltipText = $"Формирует отчет по заказам в статусе '{OrderStatus.WaitForPayment.GetEnumTitle()}'";

			entrySubdivision.SetEntityAutocompleteSelectorFactory(subdivisionSelectorFactory);
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по выставленным счетам";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "creationDate", DateTime.Now },
				{ "startDate", daterangepickerOrderCreation.StartDate.Date },
				{ "endDate", daterangepickerOrderCreation.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59) },
				{ "authorSubdivision", (entrySubdivision.Subject as Subdivision)?.Id }
			};

			var reportInfo = _reportFactory.CreateReport();
			reportInfo.Identifier = "Sales.SetBillsReport";
			reportInfo.Parameters = parameters;

			return reportInfo;
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}
	}
}
