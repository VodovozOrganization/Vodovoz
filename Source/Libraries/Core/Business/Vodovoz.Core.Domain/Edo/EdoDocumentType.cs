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
	}
}
