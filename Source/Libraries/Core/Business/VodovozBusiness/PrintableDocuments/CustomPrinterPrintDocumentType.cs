using System.ComponentModel.DataAnnotations;

namespace Vodovoz.PrintableDocuments
{
	public enum CustomPrinterPrintDocumentType
	{
		[Display(Name = "Талон погрузки (вода)")]
		WaterCarLoadDocument,
		[Display(Name = "Талон погрузки (оборуд.)")]
		EquipmentCarLoadDocument
	}
}
