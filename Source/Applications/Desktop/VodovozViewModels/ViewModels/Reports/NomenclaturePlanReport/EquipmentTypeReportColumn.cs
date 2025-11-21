using Gamma.Utilities;
using Vodovoz.Core.Domain.BasicHandbooks;

namespace Vodovoz.ViewModels.ViewModels.Reports.NomenclaturePlanReport
{
	public class EquipmentTypeReportColumn : NomenclaturePlanReportColumn
	{
		public EquipmentType EquipmentType { get; set; }
		public override string Name => EquipmentType.GetEnumTitle();
		public override NomenclaturePlanReportColumnType ColumnType => NomenclaturePlanReportColumnType.EquipmentType;
	}
}
