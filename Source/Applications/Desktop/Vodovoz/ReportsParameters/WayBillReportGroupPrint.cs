using System;
using System.Collections.Generic;
using QS.Report;
using QSReport;
using QS.Dialog.GtkUI;
using Vodovoz.Domain.Employees;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;
namespace Vodovoz.ReportsParameters
{
	public partial class WayBillReportGroupPrint : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICarJournalFactory _carJournalFactory;

		public WayBillReportGroupPrint(IEmployeeJournalFactory employeeJournalFactory, ICarJournalFactory carJournalFactory)
		{
			Build();
		}

		public string Title => throw new NotImplementedException();

		public event EventHandler<LoadReportEventArgs> LoadReport;
	}
}
