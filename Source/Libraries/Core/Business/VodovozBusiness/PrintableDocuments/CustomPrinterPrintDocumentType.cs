using System.ComponentModel.DataAnnotations;

namespace Vodovoz.PrintableDocuments
{
	public enum CustomPrinterPrintDocumentType
	{
		[Display(Name = "Талон погрузки (склад воды)")]
		WaterCarLoadDocument,
		[Display(Name = "Талон погрузки (склад оборудования)")]
		EquipmentCarLoadDocument
	}
}
