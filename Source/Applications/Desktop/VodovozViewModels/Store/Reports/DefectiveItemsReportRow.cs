using System;
using Gamma.Utilities;
using Vodovoz.Domain.Documents;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Store.Reports
{
	public partial class DefectiveItemsReport
	{
		public class DefectiveItemsReportRow
		{
			public int Id { get; set; }
			public DateTime Date { get; set; }
			public int WarehouseId { get; internal set; }
			public decimal Amount { get; set; }
			public string DefectiveItemName { get; set; }
			public string DriverLastName { get; set; }
			public int? RouteListId { get; set; }
			public Type DocumentType { get; set; }
			public int DefectTypeId { get; set; }
			public string DefectTypeName { get; set; }
			public DefectSource DefectSource { get; set; }
			public string DefectSourceName => DefectSource.GetEnumTitle();
			public string AuthorLastName { get; set; }
			public string Comment {  get; set; }
			public string DefectDetectedAt { get; set; }
			public string DocumentTypeName => DocumentType.GetClassUserFriendlyName().Nominative.CapitalizeSentence();
		}
	}
}
