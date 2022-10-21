using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.ViewModels.Reports.NomenclaturePlanReport
{
	public class NomenclaturePlanReportRow
	{
		public string Name => Employee.FullName;
		public Employee Employee { get; set; }
		public List<decimal> Columns { get; set; }
	}
}