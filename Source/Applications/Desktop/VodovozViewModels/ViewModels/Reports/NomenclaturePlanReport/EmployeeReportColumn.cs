namespace Vodovoz.ViewModels.ViewModels.Reports.NomenclaturePlanReport
{
	public class EmployeeReportColumn : NomenclaturePlanReportColumn
	{
		public string LastName { get; set; }
		public string Patronymic { get; set; }
		public string FullName => $"{ LastName } { Name } { Patronymic }";
		public override NomenclaturePlanReportColumnType ColumnType => NomenclaturePlanReportColumnType.Employee;
	}
}