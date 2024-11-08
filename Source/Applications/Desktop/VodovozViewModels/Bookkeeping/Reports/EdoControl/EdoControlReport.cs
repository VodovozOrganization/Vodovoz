using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl
{
	public partial class EdoControlReport
	{
		private EdoControlReport() { }

		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; private set; }
		public int ClosingDocumentDeliveryScheduleId { get; private set; }
		public IList<EdoControlReportRow> Rows { get; private set; } = new List<EdoControlReportRow>();

		public static async Task<EdoControlReport> Create(
			IUnitOfWork unitOfWork,
			DateTime startDate,
			DateTime endDate,
			int closingDocumentDeliveryScheduleId,
			CancellationToken cancellationToken)
		{
			var report = new EdoControlReport
			{
				StartDate = startDate,
				EndDate = endDate,
				ClosingDocumentDeliveryScheduleId = closingDocumentDeliveryScheduleId
			};

			await report.SetReportRows(unitOfWork, cancellationToken);

			return report;
		}
	}
}
