using System.ComponentModel.DataAnnotations;

namespace Vodovoz.PrintableDocuments
{
	public enum CustomPrintDocumentType
	{
		[Display(Name = "Талон погрузки (вода)")]
		WaterCarLoadDocument,
		[Display(Name = "Талон погрузки (оборуд.)")]
		EquipmentCarLoadDocument,
		[Display(Name = "Талон погрузки (контроль)")]
		ControlCarLoadDocument
	}
}
