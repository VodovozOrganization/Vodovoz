using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Тип документа ЭДО
	/// </summary>
	public enum EdoDocumentType
	{
		/// <summary>
		/// УПД
		/// </summary>
		[Display(Name = "УПД")]
		UPD,

		/// <summary>
		/// Счет
		/// </summary>
		[Display(Name = "Счет")]
		Bill,

		// Переименовать в InformalDocument
		/// <summary>
		/// Акт приёма-передачи оборудования
		/// </summary>
		[Display(Name = "Акт приёма-передачи оборудования")]
		EquipmentTransfer,
	}
}
