using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Reports.Editing.Modifiers;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public partial class EdoControlReport
	{
		private const string _dateFormatString = "dd.MM.yyyy";

		private EdoControlReport() { }

		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; private set; }
		public IList<EdoControlReportRow> Rows { get; private set; } = new List<EdoControlReportRow>();

		public static async Task<EdoControlReport> Create(
			IUnitOfWork unitOfWork,
			DateTime startDate,
			DateTime endDate,
			int closingDocumentDeliveryScheduleId,
			IncludeExludeFiltersViewModel filterViewModel,
			IEnumerable<GroupingType> selectedGroupingTypes,
			CancellationToken cancellationToken)
		{
			var report = new EdoControlReport
			{
				StartDate = startDate,
				EndDate = endDate
			};

			report.SetRequestRestrictions(filterViewModel, closingDocumentDeliveryScheduleId);
			report.SetGroupingTypes(selectedGroupingTypes);

			await report.SetReportRows(unitOfWork, cancellationToken);

			return report;
		}
	}
}
