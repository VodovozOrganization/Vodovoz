namespace Vodovoz.ViewModels.ViewModels.Reports.NomenclaturePlanReport
{
	public class NomenclatureReportColumn : NomenclaturePlanReportColumn
	{
		public int? PlanDay { get; set; }
		public int? PlanMonth { get; set; }
		public override NomenclaturePlanReportColumnType ColumnType => NomenclaturePlanReportColumnType.Nomenclature;
	}
}