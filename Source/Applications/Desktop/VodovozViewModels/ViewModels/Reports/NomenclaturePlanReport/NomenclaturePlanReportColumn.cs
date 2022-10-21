namespace Vodovoz.ViewModels.ViewModels.Reports.NomenclaturePlanReport
{
	public class NomenclaturePlanReportColumn
	{
		public int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual NomenclaturePlanReportColumnType ColumnType { get; set; }
	}
}