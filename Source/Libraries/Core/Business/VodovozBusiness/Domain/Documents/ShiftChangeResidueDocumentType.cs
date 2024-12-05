using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Documents
{
	/// <summary>
	/// Тип передачи остатков
	/// </summary>
	public enum ShiftChangeResidueDocumentType
	{
		/// <summary>
		/// По складу
		/// </summary>
		[Display(Name = "По складу")]
		Warehouse,
		/// <summary>
		/// По автомобилю
		/// </summary>
		[Display(Name = "По автомобилю")]
		Car
	}
}
