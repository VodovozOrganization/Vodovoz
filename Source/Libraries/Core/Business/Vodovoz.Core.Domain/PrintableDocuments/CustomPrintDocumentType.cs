using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.PrintableDocuments
{
	/// <summary>
	/// Дополнительные типы документов для печати
	/// </summary>
	public enum CustomPrintDocumentType
	{
		/// <summary>
		/// Талон погрузки (вода)
		/// </summary>
		[Display(Name = "Талон погрузки (вода)")]
		WaterCarLoadDocument,
		/// <summary>
		/// Талон погрузки (оборудование)
		/// </summary>
		[Display(Name = "Талон погрузки (оборуд.)")]
		EquipmentCarLoadDocument,
		/// <summary>
		/// Талон погрузки (контроль)
		/// </summary>
		[Display(Name = "Талон погрузки (контроль)")]
		ControlCarLoadDocument
	}
}
