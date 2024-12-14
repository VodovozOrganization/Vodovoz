using System;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		public class DefectiveItemsReportRow
		{
			public int Id { get; set; }
			public DateTime Date { get; set; }
			public decimal Amount { get; set; }
			public string DefectTypeName { get; set; }
			public string DefectiveItemName { get; set; }
			public string DriverSurname { get; set; }
			public int? RouteListId { get; set; }
			public DocumentType DocumentType { get; set; }
			public DefectSource DefectSource { get; set; }
			public string AuthorSurname { get; set; }
			public string Comment {  get; set; }
			public string DefectDetectedAt { get; set; }
		}
	}
}
