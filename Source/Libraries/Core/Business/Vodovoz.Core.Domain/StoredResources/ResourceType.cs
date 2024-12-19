using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.StoredResources
{
	/// <summary>
	/// Тип файла
	/// </summary>
	public enum ResourceType
	{
		/// <summary>
		/// Изображение
		/// </summary>
		[Display(Name = "Изображение")]
		Image,

		/// <summary>
		/// PDF
		/// </summary>
		[Display(Name = "PDF")]
		Pdf,

		/// <summary>
		/// Бинарный
		/// </summary>
		[Display(Name = "Бинарный")]
		Binary
	}
}
