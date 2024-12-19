using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.StoredResources
{
	/// <summary>
	/// Тип изображения
	/// </summary>
	public enum ImageType
	{
		/// <summary>
		/// Подпись
		/// </summary>
		[Display(Name = "Подпись")]
		Signature,

		/// <summary>
		/// Прочее
		/// </summary>
		[Display(Name = "Прочее")]
		Other
	}
}
